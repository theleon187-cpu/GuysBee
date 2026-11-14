using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Components;

public sealed class MenuEntry
{
    public required MenuItem Item { get; init; }
    public List<MenuItem> Children { get; init; } = new();
}

public class MenuViewComponent : ViewComponent
{
    private readonly AppDbContext _db;
    public MenuViewComponent(AppDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync(bool footer = false)
    {
        var items = await _db.MenuItems.AsNoTracking()
            .Where(m => m.IsVisible && m.IsFooter == footer)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Id)
            .ToListAsync();

        var categories = footer
            ? new List<Category>()
            : await _db.Categories.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync();

        return View(BuildEntries(items, categories, footer));
    }

    /// <summary>
    /// Construit le menu tel qu'affiché : les entrées sans parent sont les titres principaux,
    /// celles qui ont un parent s'affichent dans son menu déroulant. Les gammes de produits
    /// alimentent automatiquement le sous-menu de l'entrée « Produits ».
    /// </summary>
    public static List<MenuEntry> BuildEntries(List<MenuItem> items, List<Category> categories, bool footer)
    {
        var top = items.Where(i => i.ParentId is null).OrderBy(i => i.SortOrder).ThenBy(i => i.Id).ToList();
        if (footer)
        {
            return top.Select(i => new MenuEntry { Item = i }).ToList();
        }

        var products = FindProductsRoot(top);
        var ranges = BuildRangeItems(categories);
        if (ranges.Count > 0)
        {
            top = top.Where(i => !IsRangeLink(i) || i.Id == products?.Id).ToList();
        }

        var entries = new List<MenuEntry>();
        foreach (var item in top)
        {
            var children = items.Where(c => c.ParentId == item.Id).OrderBy(c => c.SortOrder).ThenBy(c => c.Id).ToList();
            if (products is not null && item.Id == products.Id)
            {
                item.Url = "/Produits";
                if (ranges.Count > 0) children = ranges;
            }

            entries.Add(new MenuEntry { Item = item, Children = children });
        }

        if (products is null && ranges.Count > 0)
        {
            var synthetic = new MenuItem { Label = "Produits", Url = "/Produits", IsVisible = true };
            entries.Insert(Math.Min(1, entries.Count), new MenuEntry { Item = synthetic, Children = ranges });
        }

        return entries;
    }

    public static MenuItem? FindProductsRoot(IEnumerable<MenuItem> topLevel)
    {
        var list = topLevel.ToList();
        return list.FirstOrDefault(IsProductsRoot) ?? list.FirstOrDefault(HasProductsLabel);
    }

    public static List<MenuItem> BuildRangeItems(List<Category> categories)
    {
        return categories
            .Where(c => !string.IsNullOrWhiteSpace(c.Slug))
            .Select(c => new MenuItem
            {
                Label = string.IsNullOrWhiteSpace(c.MenuLabel) ? c.Name : c.MenuLabel!,
                Url = $"/Produits?gamme={Uri.EscapeDataString(c.Slug)}",
                IsVisible = true,
                SortOrder = c.SortOrder
            })
            .OrderBy(c => c.SortOrder)
            .ToList();
    }

    public static bool IsHomeLink(MenuItem item)
    {
        var url = (item.Url ?? string.Empty).Trim();
        var label = (item.Label ?? string.Empty).Trim();
        return url == "/" || label.Contains("accueil", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsProductsRoot(MenuItem item)
    {
        var url = (item.Url ?? string.Empty).Trim();
        if (!url.StartsWith("/Produits", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !url.Contains("gamme=", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasProductsLabel(MenuItem item) =>
        (item.Label ?? string.Empty).Contains("produit", StringComparison.OrdinalIgnoreCase);

    public static bool HasExploitationLabel(MenuItem item) =>
        (item.Label ?? string.Empty).Contains("exploitation", StringComparison.OrdinalIgnoreCase);

    public static bool HasPrestationsLabel(MenuItem item) =>
        (item.Label ?? string.Empty).Contains("prestation", StringComparison.OrdinalIgnoreCase);

    public static bool HasFindUsLabel(MenuItem item)
    {
        var label = item.Label ?? string.Empty;
        return label.Contains("où nous trouver", StringComparison.OrdinalIgnoreCase)
            || label.Contains("ou nous trouver", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasContactLabel(MenuItem item) =>
        (item.Label ?? string.Empty).Contains("contact", StringComparison.OrdinalIgnoreCase);

    public static bool IsRangeLink(MenuItem item)
    {
        var url = (item.Url ?? string.Empty).Trim();
        return url.StartsWith("/Produits", StringComparison.OrdinalIgnoreCase)
            && url.Contains("gamme=", StringComparison.OrdinalIgnoreCase);
    }
}
