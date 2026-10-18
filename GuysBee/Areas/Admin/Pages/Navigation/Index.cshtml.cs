using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Navigation;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<MenuItem> Items { get; private set; } = new();

    public List<(string Group, string Label, string Url)> Links { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Items = await _db.MenuItems.AsNoTracking().OrderBy(m => m.IsFooter).ThenBy(m => m.SortOrder).ToListAsync();
        Links.Add(("Pages du site", "Accueil", "/"));
        Links.Add(("Pages du site", "Boutique (toutes les gammes)", "/Produits"));
        Links.Add(("Pages du site", "La ruche au fil des saisons", "/Calendrier"));
        Links.Add(("Pages du site", "Galerie", "/Galerie"));
        Links.Add(("Pages du site", "Où nous trouver", "/OuNousTrouver"));
        Links.Add(("Pages du site", "Contact", "/Contact"));
        foreach (var c in await _db.Categories.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync())
            Links.Add(("Gammes de produits", c.Name, "/Produits?gamme=" + c.Slug));
        foreach (var p in await _db.ContentPages.AsNoTracking().OrderBy(p => p.Title).ToListAsync())
            Links.Add(("Pages de contenu", p.Title, "/p/" + p.Slug));
    }

    private static bool ValidUrl(string url) => TextTools.IsSafeUrl(url);

    public async Task<IActionResult> OnPostSaveAsync(int id, string label, string url, bool isVisible)
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
        await _db.SaveChangesAsync();
        TempData["Message"] = $"« {m.Label} » enregistré.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, int direction)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item is null) return NotFound();
        var list = await _db.MenuItems.Where(m => m.IsFooter == item.IsFooter).OrderBy(m => m.SortOrder).ThenBy(m => m.Id).ToListAsync();
        var i = list.FindIndex(m => m.Id == id);
        var j = i + direction;
        if (j >= 0 && j < list.Count) (list[i], list[j]) = (list[j], list[i]);
        for (var k = 0; k < list.Count; k++) list[k].SortOrder = k + 1;
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddAsync(string label, string url, bool isFooter)
    {
        if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(url) || !ValidUrl(url.Trim()))
        {
            TempData["Error"] = "Indiquez un nom et un lien (commençant par / ou https://).";
            return RedirectToPage();
        }
        var max = await _db.MenuItems.Where(m => m.IsFooter == isFooter).Select(m => (int?)m.SortOrder).MaxAsync() ?? 0;
        _db.MenuItems.Add(new MenuItem { Label = label.Trim(), Url = url.Trim(), IsFooter = isFooter, SortOrder = max + 1 });
        await _db.SaveChangesAsync();
        TempData["Message"] = "Lien ajouté.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var m = await _db.MenuItems.FindAsync(id);
        if (m is not null)
        {
            _db.MenuItems.Remove(m);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Lien supprimé.";
        }
        return RedirectToPage();
    }
}
