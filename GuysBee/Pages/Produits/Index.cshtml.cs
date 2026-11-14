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
        Categories = await _db.Categories.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync();

        if (!string.IsNullOrEmpty(Gamme))
        {
            Current = Categories.FirstOrDefault(c => c.Slug == Gamme);
            if (Current is null) return NotFound();
        }

        var query = _db.Products.AsNoTracking()
            .Include(p => p.Variants)
            .Include(p => p.MainMedia)
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        if (Current is not null)
        {
            query = query.Where(p => p.CategoryId == Current.Id);
        }

        Products = await query
            .OrderBy(p => p.Category == null ? int.MaxValue : p.Category!.SortOrder)
            .ThenBy(p => p.SortOrder)
            .ToListAsync();

        return Page();
    }
}
