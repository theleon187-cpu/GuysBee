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

    public async Task OnGetAsync()
    {
        Products = await _db.Products.AsNoTracking().Include(p => p.Variants).Include(p => p.MainMedia)
            .Where(p => p.IsActive).OrderBy(p => p.SortOrder).ToListAsync();
    }
}
