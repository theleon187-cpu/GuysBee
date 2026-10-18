using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Textes;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<ContentPage> Items { get; private set; } = new();

    public async Task OnGetAsync() =>
        Items = await _db.ContentPages.AsNoTracking().OrderBy(p => p.SortOrder).ToListAsync();
}
