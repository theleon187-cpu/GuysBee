using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages;

/// <summary>Page vitrine des prestations : introduction et photo d'en-tête (page « prestations »), cartes (table Prestations).</summary>
public class PrestationsModel : PageModel
{
    private readonly AppDbContext _db;
    public PrestationsModel(AppDbContext db) => _db = db;

    public ContentPage PageData { get; private set; } = null!;
    public string IntroHtml => PageData.BodyHtml ?? string.Empty;
    public List<Prestation> Cards { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var page = await _db.ContentPages.AsNoTracking().Include(p => p.HeroMedia)
            .FirstOrDefaultAsync(p => p.Key == "prestations" && p.IsPublished);
        if (page is null) return NotFound();
        PageData = page;
        Cards = await _db.Prestations.AsNoTracking().Include(p => p.Media)
            .Where(p => p.IsVisible).OrderBy(p => p.SortOrder).ThenBy(p => p.Id).ToListAsync();
        return Page();
    }
}
