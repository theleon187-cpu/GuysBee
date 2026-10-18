using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages.Produits;

public class DetailModel : PageModel
{
    private readonly AppDbContext _db;
    public DetailModel(AppDbContext db) => _db = db;

    public Product Product { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var product = await _db.Products.AsNoTracking().Include(p => p.Variants).Include(p => p.MainMedia)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
        if (product is null) return NotFound();
        Product = product;
        return Page();
    }
}
