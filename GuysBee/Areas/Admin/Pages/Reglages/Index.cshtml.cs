using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Reglages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    /// <summary>Réglages connus, avec un libellé compréhensible.</summary>
    public static readonly (string Key, string Label, string Help)[] Known =
    [
        ("site.banner", "Bandeau d'annonce", "Message affiché en haut de toutes les pages (ex. « Récolte de printemps disponible »). Laissez vide pour le masquer."),
        ("business.phone", "Téléphone", ""),
        ("business.email", "E-mail de contact", ""),
        ("business.postalCode", "Code postal", ""),
        ("business.city", "Ville", ""),
        ("social.instagram", "Compte Instagram", "Sans le @, ex. GUYs.Bee"),
        ("social.facebook", "Page Facebook", ""),
    ];

    public Dictionary<string, string> Values { get; private set; } = new();

    public async Task OnGetAsync() =>
        Values = await _db.SiteSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);

    public async Task<IActionResult> OnPostAsync(Dictionary<string, string> values)
    {
        var existing = await _db.SiteSettings.ToDictionaryAsync(s => s.Key);
        foreach (var (key, _, _) in Known)
        {
            var value = values.TryGetValue(key, out var v) ? v?.Trim() ?? "" : "";
            if (existing.TryGetValue(key, out var setting)) setting.Value = value;
            else _db.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
        }
        await _db.SaveChangesAsync();
        TempData["Message"] = "Réglages enregistrés.";
        return RedirectToPage();
    }
}
