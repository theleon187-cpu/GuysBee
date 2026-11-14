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

    public record MapPoint(string Kind, string Name, double Lat, double Lng, string? When, string? Hours, string? Place, string? Details, string? Link);
    public List<MapPoint> MapPoints { get; private set; } = new();

    public async Task OnGetAsync()
    {
        SalePoints = await _db.SalePoints.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
        Events = await _db.MarketEvents.AsNoTracking()
            .Where(e => e.IsVisible).ToListAsync();
        Events = Events.OrderBy(e => GuysBee.Services.WeekDays.SortKey(e.DaysMask)).ThenBy(e => e.Title).ToList();
        MapPoints = Events.Where(e => e.Latitude != null && e.Longitude != null)
            .Select(e => new MapPoint("marche", e.Title, e.Latitude!.Value, e.Longitude!.Value,
                GuysBee.Services.WeekDays.Format(e.DaysMask), e.Hours, e.Place, e.Details, e.MapUrl))
            .Concat(SalePoints.Where(s => s.Latitude != null && s.Longitude != null)
                .Select(s => new MapPoint("commerce", s.Name, s.Latitude!.Value, s.Longitude!.Value,
                    null, s.Schedule, $"{s.Address}, {s.PostalCode} {s.City}".Trim(' ', ','), s.Details, s.MapUrl)))
            .ToList();
    }
}
