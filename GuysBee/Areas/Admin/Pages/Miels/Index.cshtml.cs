using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Miels;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Product> Products { get; private set; } = new();
    public List<MediaAsset> Medias { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Products = await _db.Products.Include(p => p.Variants).OrderBy(p => p.SortOrder).ToListAsync();
        Medias = await _db.MediaAssets.AsNoTracking().OrderBy(m => m.SortOrder).ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveProductAsync(int id, string name, string shortDescription,
        string? longDescription, string? origin, string? usageTips, bool isActive, int? mainMediaId)
    {
        var p = await _db.Products.FindAsync(id);
        if (p is null) return NotFound();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(shortDescription))
        {
            TempData["Error"] = "Le nom et la description courte sont obligatoires.";
            return RedirectToPage();
        }
        p.Name = name.Trim();
        p.ShortDescription = shortDescription.Trim();
        p.LongDescription = longDescription?.Trim();
        p.Origin = origin?.Trim();
        p.UsageTips = usageTips?.Trim();
        p.IsActive = isActive;
        p.MainMediaId = mainMediaId;
        p.UpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Message"] = $"« {p.Name} » enregistré.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveVariantAsync(int id, decimal price, int stock, int weight, bool isActive)
    {
        var v = await _db.ProductVariants.FindAsync(id);
        if (v is null) return NotFound();
        if (price < 0 || stock < 0 || weight < 0)
        {
            TempData["Error"] = "Les valeurs ne peuvent pas être négatives.";
            return RedirectToPage();
        }
        v.Price = price;
        v.Stock = stock;
        v.ShippingWeightGrams = weight;
        v.IsActive = isActive;
        await _db.SaveChangesAsync();
        TempData["Message"] = $"« {v.Label} » enregistré.";
        return RedirectToPage();
    }
}
