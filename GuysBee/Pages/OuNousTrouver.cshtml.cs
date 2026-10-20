using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages;

public class OuNousTrouverModel : PageModel
{
    private readonly AppDbContext _db;
    public OuNousTrouverModel(AppDbContext db) => _db = db;

    public List<SalePoint> SalePoints { get; private set; } = new();
    public List<MarketEvent> Events { get; private set; } = new();

    public async Task OnGetAsync()
    {
        SalePoints = await _db.SalePoints.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
        var today = DateTime.Today;
        Events = await _db.MarketEvents.AsNoTracking()
            .Where(e => e.IsVisible && e.EventDate >= today).OrderBy(e => e.EventDate).ToListAsync();
    }
}
