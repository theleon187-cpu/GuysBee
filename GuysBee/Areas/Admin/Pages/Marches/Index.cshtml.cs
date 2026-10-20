using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Marches;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<MarketEvent> Events { get; private set; } = new();

    public async Task OnGetAsync() =>
        Events = await _db.MarketEvents.AsNoTracking().OrderByDescending(e => e.EventDate).ToListAsync();

    public async Task<IActionResult> OnPostSaveAsync(int id, string title, DateTime? eventDate, string? hours, string? place, string? details, bool isVisible)
    {
        if (string.IsNullOrWhiteSpace(title) || eventDate is null)
        {
            TempData["Error"] = "Le titre et la date sont obligatoires.";
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
        e.EventDate = eventDate.Value.Date;
        e.Hours = hours?.Trim();
        e.Place = place?.Trim();
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
