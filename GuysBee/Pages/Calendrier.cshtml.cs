using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages;

public class CalendrierModel : PageModel
{
    private readonly AppDbContext _db;
    public CalendrierModel(AppDbContext db) => _db = db;

    public List<HiveCalendarEntry> Entries { get; private set; } = new();

    public async Task OnGetAsync() =>
        Entries = await _db.HiveCalendarEntries.AsNoTracking()
            .Where(e => e.IsVisible).OrderBy(e => e.Month).ToListAsync();
}
