using System.Globalization;
using System.Text.Json;
using GuysBee.Data;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Services;

/// <summary>
/// Place les points de vente sur la carte : coordonnées lues dans le lien Google Maps,
/// sinon recherche de l'adresse sur OpenStreetMap (Nominatim).
/// </summary>
public class GeoLocator
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<GeoLocator> _logger;

    public GeoLocator(IHttpClientFactory http, ILogger<GeoLocator> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<(double Lat, double Lng)?> LocateAsync(string? mapUrl, string? address, CancellationToken ct = default)
    {
        if (MapLinks.Coordinates(mapUrl) is { } fromLink) return fromLink;
        if (string.IsNullOrWhiteSpace(address)) return null;
        try
        {
            var client = _http.CreateClient("geo");
            var url = "https://nominatim.openstreetmap.org/search?format=json&limit=1&countrycodes=fr&q=" + Uri.EscapeDataString(address.Trim());
            using var doc = JsonDocument.Parse(await client.GetStringAsync(url, ct));
            if (doc.RootElement.GetArrayLength() == 0) return null;
            var first = doc.RootElement[0];
            return (double.Parse(first.GetProperty("lat").GetString()!, CultureInfo.InvariantCulture),
                    double.Parse(first.GetProperty("lon").GetString()!, CultureInfo.InvariantCulture));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Adresse introuvable sur la carte : {Address}", address);
            return null;
        }
    }

    /// <summary>Complète les coordonnées manquantes (quelques-unes à la fois, pour respecter le service gratuit).</summary>
    public async Task FillMissingAsync(AppDbContext db, int max = 5, CancellationToken ct = default)
    {
        var done = 0;
        foreach (var sp in await db.SalePoints.Where(s => s.Latitude == null).Take(max).ToListAsync(ct))
        {
            if (await LocateAsync(sp.MapUrl, $"{sp.Address}, {sp.PostalCode} {sp.City}", ct) is { } c) { sp.Latitude = c.Lat; sp.Longitude = c.Lng; }
            if (++done >= max) break;
            await Task.Delay(1100, ct);
        }
        foreach (var e in await db.MarketEvents.Where(m => m.Latitude == null).Take(Math.Max(0, max - done)).ToListAsync(ct))
        {
            if (await LocateAsync(e.MapUrl, e.Place, ct) is { } c) { e.Latitude = c.Lat; e.Longitude = c.Lng; }
            await Task.Delay(1100, ct);
        }
        await db.SaveChangesAsync(ct);
    }
}
