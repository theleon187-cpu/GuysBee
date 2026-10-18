using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Accueil;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IHtmlCleaner _cleaner;
    public IndexModel(AppDbContext db, IHtmlCleaner cleaner) { _db = db; _cleaner = cleaner; }

    public List<HomeSection> Sections { get; private set; } = new();

    public static string KindLabel(string kind) => kind switch
    {
        "Hero" => "Bandeau d'accueil",
        "Etapes" => "De la fleur au pot",
        "ProduitsDeSaison" => "Nos miels",
        "Calendrier" => "Le mois à la ruche",
        "Actualites" => "Actualités",
        "PointsDeVente" => "Points de vente",
        _ => "Texte libre"
    };

    public List<Category> Categories { get; private set; } = new();

    public static string? SlugOf(string? url)
    {
        const string key = "gamme=";
        var i = url?.IndexOf(key, StringComparison.Ordinal) ?? -1;
        return i < 0 ? null : url![(i + key.Length)..];
    }

    public async Task OnGetAsync()
    {
        Sections = await _db.HomeSections.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync();
        Categories = await _db.Categories.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync();
    }

    public async Task<IActionResult> OnPostAddRangeAsync()
    {
        var max = await _db.HomeSections.Select(s => (int?)s.SortOrder).MaxAsync() ?? 0;
        _db.HomeSections.Add(new HomeSection { Kind = "ProduitsDeSaison", Title = "Nos produits", ButtonLabel = "Voir tous les produits", ButtonUrl = "/Produits", SortOrder = max + 1 });
        await _db.SaveChangesAsync();
        TempData["Message"] = "Bloc de produits ajouté en bas de l'accueil.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveAsync(int id, string? title, string? subtitle, string? bodyHtml,
        string? buttonLabel, string? buttonUrl, string? categorySlug, bool isVisible, int sortOrder)
    {
        var s = await _db.HomeSections.FindAsync(id);
        if (s is null) return NotFound();
        if (s.Kind != "ProduitsDeSaison" && !string.IsNullOrWhiteSpace(buttonUrl) && !TextTools.IsSafeUrl(buttonUrl))
        {
            TempData["Error"] = "Le lien du bouton doit commencer par / ou https://.";
            return RedirectToPage();
        }
        if (s.Kind == "ProduitsDeSaison" && !string.IsNullOrEmpty(categorySlug) && !await _db.Categories.AnyAsync(c => c.Slug == categorySlug))
        {
            TempData["Error"] = "Cette gamme n'existe plus.";
            return RedirectToPage();
        }
        s.Title = title?.Trim();
        s.Subtitle = subtitle?.Trim();
        if (s.Kind == "Texte") s.BodyHtml = _cleaner.Clean(bodyHtml);
        s.ButtonLabel = buttonLabel?.Trim();
        s.ButtonUrl = s.Kind == "ProduitsDeSaison"
            ? (string.IsNullOrEmpty(categorySlug) ? "/Produits" : "/Produits?gamme=" + categorySlug)
            : buttonUrl?.Trim();
        s.IsVisible = isVisible;
        s.SortOrder = sortOrder;
        await _db.SaveChangesAsync();
        TempData["Message"] = $"Bloc « {KindLabel(s.Kind)} » enregistré.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, int direction)
    {
        var list = await _db.HomeSections.OrderBy(s => s.SortOrder).ThenBy(s => s.Id).ToListAsync();
        var i = list.FindIndex(s => s.Id == id);
        var j = i + direction;
        if (i >= 0 && j >= 0 && j < list.Count)
            (list[i], list[j]) = (list[j], list[i]);
        for (var k = 0; k < list.Count; k++) list[k].SortOrder = k + 1;
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddTextAsync()
    {
        var max = await _db.HomeSections.Select(s => (int?)s.SortOrder).MaxAsync() ?? 0;
        _db.HomeSections.Add(new HomeSection { Kind = "Texte", Title = "Nouveau texte", BodyHtml = "<p></p>", SortOrder = max + 1 });
        await _db.SaveChangesAsync();
        TempData["Message"] = "Bloc de texte ajouté en bas de l'accueil.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var s = await _db.HomeSections.FindAsync(id);
        if (s is { Kind: "Texte" } || (s is { Kind: "ProduitsDeSaison" } && await _db.HomeSections.CountAsync(x => x.Kind == "ProduitsDeSaison") > 1))
        {
            _db.HomeSections.Remove(s);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Bloc supprimé.";
        }
        return RedirectToPage();
    }
}
