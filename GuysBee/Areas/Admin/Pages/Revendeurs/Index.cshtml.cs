using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Revendeurs;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly GuysBee.Services.MediaService _media;
    public IndexModel(AppDbContext db, GuysBee.Services.MediaService media) { _db = db; _media = media; }

    public List<SalePoint> Points { get; private set; } = new();

    public async Task OnGetAsync() =>
        Points = await _db.SalePoints.AsNoTracking().OrderBy(s => s.Name).ToListAsync();

    public async Task<IActionResult> OnPostSaveAsync(int id, string name, string? address, string? postalCode, string? city, string? schedule, string? phone,
        string? mapUrl, string? details, bool isActive, IFormFile? photo, bool removePhoto, [FromServices] GuysBee.Services.GeoLocator geo)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Le nom est obligatoire.";
            return RedirectToPage();
        }
        SalePoint? s;
        if (id == 0)
        {
            s = new SalePoint();
            _db.SalePoints.Add(s);
        }
        else
        {
            s = await _db.SalePoints.FindAsync(id);
            if (s is null) return NotFound();
        }
        s.Name = name.Trim();
        s.Address = address?.Trim() ?? "";
        s.PostalCode = postalCode?.Trim() ?? "";
        s.City = city?.Trim() ?? "";
        s.Schedule = schedule?.Trim();
        s.Phone = phone?.Trim();
        s.Details = details?.Trim();
        s.MapUrl = GuysBee.Services.MapLinks.Clean(mapUrl);
        var spot = await geo.LocateAsync(s.MapUrl, $"{s.Address}, {s.PostalCode} {s.City}");
        s.Latitude = spot?.Lat;
        s.Longitude = spot?.Lng;
        if (removePhoto) s.PhotoBaseName = null;
        if (photo is { Length: > 0 })
        {
            try { s.PhotoBaseName = await _media.SavePhotoAsync(photo) ?? s.PhotoBaseName; }
            catch (Exception ex) { TempData["Error"] = ex.Message; return RedirectToPage(); }
        }
        s.IsActive = isActive;
        await _db.SaveChangesAsync();
        TempData["Message"] = spot is null
            ? "Lieu de revente enregistré, mais introuvable sur la carte : vérifiez l'adresse ou collez un lien Google Maps."
            : "Lieu de revente enregistré et placé sur la carte.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLocateAllAsync([FromServices] GuysBee.Services.GeoLocator geo)
    {
        await geo.FillMissingAsync(_db);
        TempData["Message"] = "Les lieux ont été placés sur la carte quand leur adresse a été trouvée.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var s = await _db.SalePoints.FindAsync(id);
        if (s is not null)
        {
            _db.SalePoints.Remove(s);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Lieu de revente supprimé.";
        }
        return RedirectToPage();
    }
}
