using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages;

/// <summary>Affiche une page éditable depuis l'admin (histoire, CGV, mentions...).</summary>
public class ContenuModel : PageModel
{
    private readonly AppDbContext _db;
    public ContenuModel(AppDbContext db) => _db = db;

    public ContentPage PageData { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var page = await _db.ContentPages.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);
        if (page is null) return NotFound();
        PageData = page;
        return Page();
    }
}
