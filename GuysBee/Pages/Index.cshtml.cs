using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<HomeSection> Sections { get; private set; } = new();
    public List<Product> Products { get; private set; } = new();
    public HiveCalendarEntry? CurrentMonth { get; private set; }
    public MediaAsset? HeroMedia { get; private set; }

    public async Task OnGetAsync()
    {
        Sections = await _db.HomeSections.AsNoTracking().Include(s => s.Media)
            .Where(s => s.IsVisible).OrderBy(s => s.SortOrder).ToListAsync();

        Products = await _db.Products.AsNoTracking().Include(p => p.Variants).Include(p => p.MainMedia)
            .Where(p => p.IsActive).OrderBy(p => p.SortOrder).ToListAsync();

        var month = DateTime.Now.Month;
        CurrentMonth = await _db.HiveCalendarEntries.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Month == month && c.IsVisible);

        var hero = Sections.FirstOrDefault(s => s.Kind == "Hero");
        HeroMedia = hero?.Media ?? await _db.MediaAssets.AsNoTracking()
            .Where(m => m.Status == MediaStatus.Published).OrderBy(m => m.SortOrder).FirstOrDefaultAsync();
    }
}
