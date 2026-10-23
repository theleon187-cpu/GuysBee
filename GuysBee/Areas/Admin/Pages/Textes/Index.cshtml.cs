using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Textes;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly FontLibrary _fonts;
    public IndexModel(AppDbContext db, FontLibrary fonts) { _db = db; _fonts = fonts; }

    public List<(string Id, string Label, string Stack)> Fonts => _fonts.All();
    public List<(string Id, string Label, string Stack)> CustomFonts => _fonts.Custom();
    [BindProperty] public IFormFile? FontFile { get; set; }

    public List<ContentPage> Items { get; private set; } = new();

    [BindProperty] public string? TitleFont { get; set; }
    [BindProperty] public string? TextFont { get; set; }
    [BindProperty] public string? BrandFont { get; set; }

    public async Task OnGetAsync()
    {
        Items = await _db.ContentPages.AsNoTracking().OrderBy(p => p.SortOrder).ToListAsync();
        var s = await _db.SiteSettings.AsNoTracking()
            .Where(x => x.Key == FontChoices.TitleKey || x.Key == FontChoices.TextKey || x.Key == FontChoices.BrandKey)
            .ToDictionaryAsync(x => x.Key, x => x.Value);
        BrandFont = s.GetValueOrDefault(FontChoices.BrandKey);
        TitleFont = s.GetValueOrDefault(FontChoices.TitleKey);
        TextFont = s.GetValueOrDefault(FontChoices.TextKey);
    }

    public async Task<IActionResult> OnPostFontsAsync()
    {
        var existing = await _db.SiteSettings
            .Where(x => x.Key == FontChoices.TitleKey || x.Key == FontChoices.TextKey || x.Key == FontChoices.BrandKey)
            .ToDictionaryAsync(x => x.Key);
        Save(existing, FontChoices.BrandKey, _fonts.IsValid(BrandFont) ? BrandFont : "");
        Save(existing, FontChoices.TitleKey, _fonts.IsValid(TitleFont) ? TitleFont : "");
        Save(existing, FontChoices.TextKey, _fonts.IsValid(TextFont) ? TextFont : "");
        await _db.SaveChangesAsync();
        TempData["Message"] = "Polices enregistrées.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUploadFontAsync()
    {
        if (FontFile is null) TempData["Message"] = "Choisissez d'abord un fichier de police.";
        else TempData["Message"] = await _fonts.SaveAsync(FontFile) ?? "Police importée. Vous pouvez maintenant la choisir dans les listes.";
        return RedirectToPage();
    }

    public IActionResult OnPostDeleteFont(string id)
    {
        _fonts.Delete(id);
        TempData["Message"] = "Police supprimée.";
        return RedirectToPage();
    }

    private void Save(Dictionary<string, SiteSetting> existing, string key, string? value)
    {
        value ??= "";
        if (existing.TryGetValue(key, out var setting)) setting.Value = value;
        else _db.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
    }
}
