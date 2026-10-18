using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages.Produits;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Product> Products { get; private set; } = new();
    public List<Category> Categories { get; private set; } = new();
    public Category? Current { get; private set; }

    [BindProperty(SupportsGet = true, Name = "gamme")]
    public string? Gamme { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var usedIds = await _db.Products.Where(p => p.IsActive && p.CategoryId != null).Select(p => p.CategoryId!.Value).Distinct().ToListAsync();
        Categories = await _db.Categories.AsNoTracking().Where(c => usedIds.Contains(c.Id)).OrderBy(c => c.SortOrder).ToListAsync();

        if (!string.IsNullOrEmpty(Gamme))
        {
            Current = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == Gamme);
            if (Current is null) return NotFound();
        }

        var query = _db.Products.AsNoTracking().Include(p => p.Variants).Include(p => p.MainMedia).Include(p => p.Category).Where(p => p.IsActive);
        if (Current is not null) query = query.Where(p => p.CategoryId == Current.Id);
        Products = await query.OrderBy(p => p.SortOrder).ToListAsync();
        return Page();
    }
}
