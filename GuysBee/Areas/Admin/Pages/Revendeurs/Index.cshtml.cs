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
        string? mapUrl, string? details, bool isActive, IFormFile? photo, bool removePhoto)
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
        if (removePhoto) s.PhotoBaseName = null;
        if (photo is { Length: > 0 })
        {
            try { s.PhotoBaseName = await _media.SavePhotoAsync(photo) ?? s.PhotoBaseName; }
            catch (Exception ex) { TempData["Error"] = ex.Message; return RedirectToPage(); }
        }
        s.IsActive = isActive;
        await _db.SaveChangesAsync();
        TempData["Message"] = "Lieu de revente enregistré.";
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
