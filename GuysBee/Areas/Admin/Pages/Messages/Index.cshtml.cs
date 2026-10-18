using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Messages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<ContactMessage> Items { get; private set; } = new();

    public async Task OnGetAsync() =>
        Items = await _db.ContactMessages.AsNoTracking().Where(m => !m.IsArchived)
            .OrderByDescending(m => m.CreatedUtc).ToListAsync();

    public async Task<IActionResult> OnPostReadAsync(int id)
    {
        var m = await _db.ContactMessages.FindAsync(id);
        if (m is not null) { m.IsRead = true; await _db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostArchiveAsync(int id)
    {
        var m = await _db.ContactMessages.FindAsync(id);
        if (m is not null) { m.IsArchived = true; m.IsRead = true; await _db.SaveChangesAsync(); }
        TempData["Message"] = "Message archivé.";
        return RedirectToPage();
    }
}
