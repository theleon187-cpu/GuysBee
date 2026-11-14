using System.ComponentModel.DataAnnotations;
using GuysBee.Data;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Textes;

public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IHtmlCleaner _cleaner;

    public EditModel(AppDbContext db, IHtmlCleaner cleaner)
    {
        _db = db;
        _cleaner = cleaner;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    public string Slug { get; private set; } = "";
    public string PublicUrl => Slug == "prestations" ? "/Prestations" : "/p/" + Slug;
    public string? HeroBaseName { get; private set; }
    public bool HasHero => HeroBaseName is not null;

    public class InputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le titre est obligatoire."), StringLength(150)]
        public string Title { get; set; } = "";

        [StringLength(300)]
        public string? MetaDescription { get; set; }

        public string? BodyHtml { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var p = await _db.ContentPages.Include(x => x.HeroMedia).FirstOrDefaultAsync(x => x.Id == id);
        if (p is null) return NotFound();
        Slug = p.Slug;
        HeroBaseName = p.HeroMedia?.BaseName;
        Input = new InputModel
        {
            Id = p.Id,
            BodyHtml = p.BodyHtml, IsPublished = p.IsPublished
        };
        return Page();
    }

    public async Task<IActionResult> OnPostImageAsync(IFormFile? file, [FromServices] MediaService media)
    {
        if (file is not { Length: > 0 }) return BadRequest(new { error = "Aucune photo reçue." });
        try
        {
            var baseName = await media.SavePhotoAsync(file);
            return baseName is null
                ? BadRequest(new { error = "Photo illisible." })
                : new JsonResult(new { location = MediaUrls.Src(baseName, 1200) });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? heroPhoto, bool removeHero, [FromServices] MediaService media)
    {
        var p = await _db.ContentPages.Include(x => x.HeroMedia).FirstOrDefaultAsync(x => x.Id == Input.Id);
        if (p is null) return NotFound();
        Slug = p.Slug;
        HeroBaseName = p.HeroMedia?.BaseName;
        if (!ModelState.IsValid) return Page();

        if (removeHero) p.HeroMediaId = null;
        if (heroPhoto is { Length: > 0 })
        {
            try
            {
                var baseName = await media.SavePhotoAsync(heroPhoto);
                var asset = baseName is null ? null : await _db.MediaAssets.FirstOrDefaultAsync(m => m.BaseName == baseName);
                if (asset is not null) p.HeroMediaId = asset.Id;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }

        p.Title = Input.Title.Trim();
        p.MetaDescription = Input.MetaDescription?.Trim();
        p.BodyHtml = _cleaner.Clean(Input.BodyHtml);
        p.IsPublished = Input.IsPublished;
        p.UpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Message"] = "Page enregistrée.";
        return RedirectToPage("Index");
    }
}
