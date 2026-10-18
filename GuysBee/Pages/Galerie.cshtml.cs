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

    public async Task OnGetAsync() =>
        Items = await _db.MediaAssets.AsNoTracking()
            .Where(m => m.Status == MediaStatus.Published).OrderBy(m => m.SortOrder).ToListAsync();
}
