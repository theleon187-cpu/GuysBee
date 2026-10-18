using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Calendrier;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<HiveCalendarEntry> Entries { get; private set; } = new();

    public static readonly string[] MonthNames =
        ["", "Janvier", "Février", "Mars", "Avril", "Mai", "Juin", "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre"];

    public async Task OnGetAsync() =>
        Entries = await _db.HiveCalendarEntries.AsNoTracking().OrderBy(e => e.Month).ToListAsync();

    public async Task<IActionResult> OnPostSaveAsync(int id, string title, string? whatBeesDo, string? whatBeekeeperDoes, bool isVisible)
    {
        var e = await _db.HiveCalendarEntries.FindAsync(id);
        if (e is null) return NotFound();
        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["Error"] = "Le titre est obligatoire.";
            return RedirectToPage();
        }
        e.Title = title.Trim();
        e.WhatBeesDo = whatBeesDo?.Trim() ?? "";
        e.WhatBeekeeperDoes = whatBeekeeperDoes?.Trim() ?? "";
        e.IsVisible = isVisible;
        await _db.SaveChangesAsync();
        TempData["Message"] = $"{MonthNames[Math.Clamp(e.Month, 1, 12)]} enregistré.";
        return RedirectToPage();
    }
}
