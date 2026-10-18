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
    public List<MediaAsset> HeroMedias { get; private set; } = new();
    public List<MediaAsset> GalleryTeaser { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Sections = await _db.HomeSections.AsNoTracking().Include(s => s.Media)
            .Where(s => s.IsVisible).OrderBy(s => s.SortOrder).ToListAsync();

        Products = await _db.Products.AsNoTracking().Include(p => p.Variants).Include(p => p.MainMedia).Include(p => p.Category)
            .Where(p => p.IsActive).OrderBy(p => p.SortOrder).ToListAsync();

        var month = DateTime.Now.Month;
        CurrentMonth = await _db.HiveCalendarEntries.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Month == month && c.IsVisible);

        GalleryTeaser = await _db.MediaAssets.AsNoTracking()
            .Where(m => m.Status == MediaStatus.Published).OrderBy(m => m.SortOrder).Take(6).ToListAsync();

        var hero = Sections.FirstOrDefault(s => s.Kind == "Hero");
        HeroMedias = await _db.MediaAssets.AsNoTracking()
            .Where(m => m.Status == MediaStatus.Published && m.Kind == MediaKind.Image && m.ShowInHero)
            .OrderBy(m => m.SortOrder).Take(5).ToListAsync();
        if (HeroMedias.Count == 0)
        {
            var fallback = hero?.Media ?? await _db.MediaAssets.AsNoTracking()
                .Where(m => m.Status == MediaStatus.Published && m.Kind == MediaKind.Image).OrderBy(m => m.SortOrder).FirstOrDefaultAsync();
            if (fallback is not null) HeroMedias.Add(fallback);
        }
        HeroMedia = HeroMedias.FirstOrDefault();
    }
}
