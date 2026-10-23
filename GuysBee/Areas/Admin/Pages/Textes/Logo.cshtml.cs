using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Textes;

public class LogoModel : PageModel
{
    public const string HtmlKey = "site.brandHtml";

    private readonly AppDbContext _db;
    private readonly IHtmlCleaner _cleaner;
    private readonly FontLibrary _fonts;

    public LogoModel(AppDbContext db, IHtmlCleaner cleaner, FontLibrary fonts)
    {
        _db = db;
        _cleaner = cleaner;
        _fonts = fonts;
    }

    public List<(string Id, string Label, string Stack)> Fonts => _fonts.All();

    [BindProperty] public string? BrandHtml { get; set; }
    [BindProperty] public string? BrandFont { get; set; }

    public async Task OnGetAsync()
    {
        var s = await _db.SiteSettings.AsNoTracking()
            .Where(x => x.Key == HtmlKey || x.Key == "site.brandName" || x.Key == FontChoices.BrandKey)
            .ToDictionaryAsync(x => x.Key, x => x.Value);
        BrandHtml = s.GetValueOrDefault(HtmlKey);
        if (string.IsNullOrWhiteSpace(BrandHtml)) BrandHtml = System.Net.WebUtility.HtmlEncode(s.GetValueOrDefault("site.brandName"));
        BrandFont = s.GetValueOrDefault(FontChoices.BrandKey);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var existing = await _db.SiteSettings
            .Where(x => x.Key == HtmlKey || x.Key == "site.brandName" || x.Key == FontChoices.BrandKey)
            .ToDictionaryAsync(x => x.Key);
        Save(existing, HtmlKey, _cleaner.Clean(BrandHtml) ?? "");
        Save(existing, "site.brandName", "");
        Save(existing, FontChoices.BrandKey, _fonts.IsValid(BrandFont) ? BrandFont : "");
        await _db.SaveChangesAsync();
        TempData["Message"] = "Texte du logo enregistré.";
        return RedirectToPage();
    }

    private void Save(Dictionary<string, SiteSetting> existing, string key, string? value)
    {
        value ??= "";
        if (existing.TryGetValue(key, out var setting)) setting.Value = value;
        else _db.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
    }
}
