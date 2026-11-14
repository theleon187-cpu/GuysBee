using GuysBee.Components;
using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Navigation;

public sealed record MenuRowModel(IndexModel Page, MenuItem Item, int Position, int Count, bool IsChild);

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<MenuItem> Items { get; private set; } = new();
    public List<Category> Ranges { get; private set; } = new();
    public int? ProductsParentId { get; private set; }
    public List<(string Group, string Label, string Url)> Links { get; private set; } = new();

    public bool IsProductsParent(MenuItem m) => !m.IsFooter && m.Id == ProductsParentId;

    private bool IsHiddenRange(MenuItem m) =>
        Ranges.Count > 0 && MenuViewComponent.IsRangeLink(m) && m.Id != ProductsParentId;

    public List<MenuItem> TopLevel(bool footer) => Items
        .Where(i => i.IsFooter == footer && i.ParentId is null && !IsHiddenRange(i))
        .OrderBy(i => i.SortOrder).ThenBy(i => i.Id).ToList();

    public List<MenuItem> ChildrenOf(int id) => Items
        .Where(i => i.ParentId == id)
        .OrderBy(i => i.SortOrder).ThenBy(i => i.Id).ToList();

    public async Task OnGetAsync()
    {
        Items = await _db.MenuItems.AsNoTracking().OrderBy(m => m.IsFooter).ThenBy(m => m.SortOrder).ThenBy(m => m.Id).ToListAsync();
        Ranges = await _db.Categories.AsNoTracking().OrderBy(c => c.SortOrder).ThenBy(c => c.Id).ToListAsync();
        var mainTop = Items.Where(i => !i.IsFooter && i.IsVisible && i.ParentId is null);
        ProductsParentId = MenuViewComponent.FindProductsRoot(mainTop)?.Id;

        Links.Add(("Pages du site", "Accueil", "/"));
        Links.Add(("Pages du site", "Boutique (toutes les gammes)", "/Produits"));
        Links.Add(("Pages du site", "La ruche au fil des saisons", "/Calendrier"));
        Links.Add(("Pages du site", "Galerie", "/Galerie"));
        Links.Add(("Pages du site", "Où nous trouver", "/OuNousTrouver"));
        Links.Add(("Pages du site", "Contact", "/Contact"));
        foreach (var c in Ranges)
            Links.Add(("Gammes de produits", c.Name, "/Produits?gamme=" + c.Slug));
        foreach (var p in await _db.ContentPages.AsNoTracking().OrderBy(p => p.Title).ToListAsync())
            Links.Add(("Pages de contenu", p.Title, p.Slug == "prestations" ? "/Prestations" : "/p/" + p.Slug));
    }

    private static bool ValidUrl(string url) => TextTools.IsSafeUrl(url);

    private async Task<int> NextOrderAsync(bool isFooter, int? parentId)
    {
        var max = await _db.MenuItems
            .Where(m => m.IsFooter == isFooter && m.ParentId == parentId)
            .Select(m => (int?)m.SortOrder).MaxAsync();
        return (max ?? 0) + 1;
    }

    private async Task<int?> ValidParentAsync(MenuItem item, int? parentId)
    {
        if (parentId is null || item.IsFooter || parentId == item.Id) return null;
        if (await _db.MenuItems.AnyAsync(m => m.ParentId == item.Id && m.Id != item.Id)) return null;
        var parent = await _db.MenuItems.FindAsync(parentId.Value);
        return parent is { ParentId: null, IsFooter: false } ? parent.Id : null;
    }

    public async Task<IActionResult> OnPostSaveAsync(int id, string label, string url, bool isVisible, int? parentId)
    {
        var m = await _db.MenuItems.FindAsync(id);
        if (m is null) return NotFound();
        if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(url) || !ValidUrl(url.Trim()))
        {
            TempData["Error"] = "Indiquez un nom et un lien (commençant par / ou https://).";
            return RedirectToPage();
        }
        m.Label = label.Trim();
        m.Url = url.Trim();
        m.IsVisible = isVisible;
        var newParent = await ValidParentAsync(m, parentId);
        if (newParent != m.ParentId)
        {
            m.ParentId = newParent;
            m.SortOrder = await NextOrderAsync(m.IsFooter, newParent);
        }
        await _db.SaveChangesAsync();
        TempData["Message"] = $"« {m.Label} » enregistré.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, int direction)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item is null) return NotFound();
        var list = await _db.MenuItems
            .Where(m => m.IsFooter == item.IsFooter && m.ParentId == item.ParentId)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Id).ToListAsync();
        if (item.ParentId is null && !item.IsFooter && await _db.Categories.AnyAsync())
        {
            var products = MenuViewComponent.FindProductsRoot(list.Where(m => m.IsVisible));
            list = list.Where(m => !MenuViewComponent.IsRangeLink(m) || m.Id == products?.Id).ToList();
        }
        var i = list.FindIndex(m => m.Id == id);
        var j = i + direction;
        if (i >= 0 && j >= 0 && j < list.Count) (list[i], list[j]) = (list[j], list[i]);
        for (var k = 0; k < list.Count; k++) list[k].SortOrder = k + 1;
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddAsync(string label, string url, bool isFooter, int? parentId)
    {
        if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(url) || !ValidUrl(url.Trim()))
        {
            TempData["Error"] = "Indiquez un nom et un lien (commençant par / ou https://).";
            return RedirectToPage();
        }
        var item = new MenuItem { Label = label.Trim(), Url = url.Trim(), IsFooter = isFooter };
        item.ParentId = await ValidParentAsync(item, parentId);
        item.SortOrder = await NextOrderAsync(isFooter, item.ParentId);
        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Lien ajouté.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var m = await _db.MenuItems.FindAsync(id);
        if (m is not null)
        {
            var children = await _db.MenuItems.Where(c => c.ParentId == id).ToListAsync();
            var order = await NextOrderAsync(m.IsFooter, null);
            foreach (var c in children) { c.ParentId = null; c.SortOrder = order++; }
            _db.MenuItems.Remove(m);
            await _db.SaveChangesAsync();
            TempData["Message"] = children.Count > 0
                ? "Lien supprimé. Ses sous-liens sont revenus dans le menu principal."
                : "Lien supprimé.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveRangeAsync(int id, string? menuLabel)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        c.MenuLabel = string.IsNullOrWhiteSpace(menuLabel) ? null : menuLabel.Trim();
        await _db.SaveChangesAsync();
        TempData["Message"] = $"Sous-menu « {c.MenuLabel ?? c.Name} » enregistré.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveRangeAsync(int id, int direction)
    {
        var list = await _db.Categories.OrderBy(c => c.SortOrder).ThenBy(c => c.Id).ToListAsync();
        var i = list.FindIndex(c => c.Id == id);
        var j = i + direction;
        if (i >= 0 && j >= 0 && j < list.Count) (list[i], list[j]) = (list[j], list[i]);
        for (var k = 0; k < list.Count; k++) list[k].SortOrder = k + 1;
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }
}
