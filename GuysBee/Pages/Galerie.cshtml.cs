using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages;

public class GalerieModel : PageModel
{
    private readonly AppDbContext _db;
    public GalerieModel(AppDbContext db) => _db = db;

    public List<MediaAsset> Items { get; private set; } = new();
    public string Instagram { get; private set; } = "";
    public string Facebook { get; private set; } = "";

    public async Task OnGetAsync()
    {
        Items = await _db.MediaAssets.AsNoTracking()
            .Where(m => m.Status == MediaStatus.Published).OrderBy(m => m.SortOrder).ToListAsync();
        var s = await _db.SiteSettings.AsNoTracking()
            .Where(x => x.Key == "social.instagram" || x.Key == "social.facebook")
            .ToDictionaryAsync(x => x.Key, x => x.Value);
        Instagram = s.GetValueOrDefault("social.instagram") ?? "";
        Facebook = s.GetValueOrDefault("social.facebook") ?? "";
    }
}
