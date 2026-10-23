using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Marches;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly MediaService _media;
    public IndexModel(AppDbContext db, MediaService media) { _db = db; _media = media; }

    public List<MarketEvent> Events { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var list = await _db.MarketEvents.AsNoTracking().ToListAsync();
        Events = list.OrderBy(e => WeekDays.SortKey(e.DaysMask)).ThenBy(e => e.Title).ToList();
    }

    public async Task<IActionResult> OnPostSaveAsync(int id, string title, int[]? weekDays, string? hours, string? place, string? mapUrl,
        string? details, bool isVisible, IFormFile? photo, bool removePhoto)
    {
        var mask = WeekDays.ToMask(weekDays);
        if (string.IsNullOrWhiteSpace(title) || mask == 0)
        {
            TempData["Error"] = "Le titre et au moins un jour de la semaine sont obligatoires.";
            return RedirectToPage();
        }
        MarketEvent? e;
        if (id == 0)
        {
            e = new MarketEvent();
            _db.MarketEvents.Add(e);
        }
        else
        {
            e = await _db.MarketEvents.FindAsync(id);
            if (e is null) return NotFound();
        }
        e.Title = title.Trim();
        e.DaysMask = mask;
        e.WeekDay = WeekDays.FirstDay(mask);
        e.Hours = hours?.Trim();
        e.Place = place?.Trim();
        e.MapUrl = MapLinks.Clean(mapUrl);
        if (removePhoto) e.PhotoBaseName = null;
        if (photo is { Length: > 0 })
        {
            try { e.PhotoBaseName = await _media.SavePhotoAsync(photo) ?? e.PhotoBaseName; }
            catch (Exception ex) { TempData["Error"] = ex.Message; return RedirectToPage(); }
        }
        e.Details = details?.Trim();
        e.IsVisible = isVisible;
        await _db.SaveChangesAsync();
        TempData["Message"] = "Rendez-vous enregistré.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var e = await _db.MarketEvents.FindAsync(id);
        if (e is not null)
        {
            _db.MarketEvents.Remove(e);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Rendez-vous supprimé.";
        }
        return RedirectToPage();
    }
}
