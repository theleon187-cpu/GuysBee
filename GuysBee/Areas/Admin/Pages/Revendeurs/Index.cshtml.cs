using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Revendeurs;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<SalePoint> Points { get; private set; } = new();

    public async Task OnGetAsync() =>
        Points = await _db.SalePoints.AsNoTracking().OrderBy(s => s.Name).ToListAsync();

    public async Task<IActionResult> OnPostSaveAsync(int id, string name, string? address, string? postalCode, string? city, string? schedule, string? phone, bool isActive)
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
