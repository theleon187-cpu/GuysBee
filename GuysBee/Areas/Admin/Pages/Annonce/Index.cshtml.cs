using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Annonce;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public Dictionary<string, string> Values { get; private set; } = new();

    public string V(string key) => Values.TryGetValue(key, out var v) ? v : "";

    public async Task OnGetAsync() =>
        Values = await _db.SiteSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);

    public async Task<IActionResult> OnPostAsync(Dictionary<string, string> values, bool enabled, bool closable, bool resetCookie)
    {
        string G(string k) => values.TryGetValue(k, out var v) ? v?.Trim() ?? "" : "";

        var link = G(AnnouncementKeys.LinkUrl);
        if (link.Length > 0 && !link.StartsWith("/") && !link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Le lien doit commencer par / (page du site) ou https://.";
            return RedirectToPage();
        }
        if (link.StartsWith("//"))
        {
            TempData["Error"] = "Le lien doit commencer par / (page du site) ou https://.";
            return RedirectToPage();
        }
        foreach (var key in new[] { AnnouncementKeys.Start, AnnouncementKeys.End })
        {
            if (G(key).Length > 0 && !AnnouncementResolver.TryDate(G(key), out _))
            {
                TempData["Error"] = "Une date n'est pas valide.";
                return RedirectToPage();
            }
        }
        if (AnnouncementResolver.TryDate(G(AnnouncementKeys.Start), out var s0)
            && AnnouncementResolver.TryDate(G(AnnouncementKeys.End), out var e0) && e0 < s0)
        {
            TempData["Error"] = "La date de fin doit être après la date de début.";
            return RedirectToPage();
        }

        var existing = await _db.SiteSettings.ToDictionaryAsync(s => s.Key);
        string Old(string k) => existing.TryGetValue(k, out var s) ? s.Value : "";

        var contentChanged = G(AnnouncementKeys.Message) != Old(AnnouncementKeys.Message)
            || G(AnnouncementKeys.Title) != Old(AnnouncementKeys.Title);
        var version = Old(AnnouncementKeys.Version);
        if (resetCookie || contentChanged || string.IsNullOrEmpty(version))
            version = DateTime.UtcNow.Ticks.ToString();

        var final = new Dictionary<string, string>
        {
            [AnnouncementKeys.Enabled] = enabled ? "1" : "0",
            [AnnouncementKeys.Closable] = closable ? "1" : "0",
            [AnnouncementKeys.Version] = version,
        };
        foreach (var key in AnnouncementKeys.All)
            if (!final.ContainsKey(key)) final[key] = G(key);

        foreach (var (key, value) in final)
        {
            if (existing.TryGetValue(key, out var setting)) setting.Value = value;
            else _db.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
        }
        await _db.SaveChangesAsync();
        TempData["Message"] = "Annonce enregistrée.";
        return RedirectToPage();
    }
}
