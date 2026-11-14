using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Calendrier;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly GuysBee.Services.MediaService _media;
    private readonly GuysBee.Services.IHtmlCleaner _cleaner;
    public IndexModel(AppDbContext db, GuysBee.Services.MediaService media, GuysBee.Services.IHtmlCleaner cleaner) { _db = db; _media = media; _cleaner = cleaner; }

    public List<HiveCalendarEntry> Entries { get; private set; } = new();

    public static readonly string[] MonthNames =
        ["", "Janvier", "Février", "Mars", "Avril", "Mai", "Juin", "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"];

    public async Task OnGetAsync() =>
        Entries = await _db.HiveCalendarEntries.AsNoTracking().Include(e => e.Media).OrderBy(e => e.Month).ToListAsync();

    public async Task<IActionResult> OnPostSaveAsync(int id, string title, string? whatBeesDo, string? whatBeekeeperDoes, bool isVisible,
        IFormFile? photo, bool removePhoto)
    {
        var e = await _db.HiveCalendarEntries.FindAsync(id);
        if (e is null) return NotFound();
        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["Error"] = "Le titre est obligatoire.";
            return RedirectToPage();
        }
        e.Title = title.Trim();
        e.WhatBeesDo = _cleaner.Clean(whatBeesDo);
        e.WhatBeekeeperDoes = _cleaner.Clean(whatBeekeeperDoes);
        e.IsVisible = isVisible;
        if (removePhoto) e.MediaId = null;
        if (photo is { Length: > 0 })
        {
            try
            {
                var baseName = await _media.SavePhotoAsync(photo);
                var asset = baseName is null ? null : await _db.MediaAssets.FirstOrDefaultAsync(m => m.BaseName == baseName);
                if (asset is not null) e.MediaId = asset.Id;
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; return RedirectToPage(); }
        }
        await _db.SaveChangesAsync();
        TempData["Message"] = $"{MonthNames[Math.Clamp(e.Month, 1, 12)]} enregistré.";
        return RedirectToPage();
    }
}
