using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public int UnreadMessages { get; private set; }
    public int DraftPhotos { get; private set; }
    public int PublishedPhotos { get; private set; }
    public int OutOfStock { get; private set; }

    public async Task OnGetAsync()
    {
        UnreadMessages = await _db.ContactMessages.CountAsync(m => !m.IsRead && !m.IsArchived);
        DraftPhotos = await _db.MediaAssets.CountAsync(m => m.Status == MediaStatus.Draft);
        PublishedPhotos = await _db.MediaAssets.CountAsync(m => m.Status == MediaStatus.Published);
        OutOfStock = await _db.ProductVariants.CountAsync(v => v.IsActive && v.Stock <= 0);
    }
}
