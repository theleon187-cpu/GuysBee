using System.ComponentModel.DataAnnotations;
using GuysBee.Data;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        var p = await _db.ContentPages.FindAsync(id);
        if (p is null) return NotFound();
        Slug = p.Slug;
        Input = new InputModel
        {
            Id = p.Id, Title = p.Title, MetaDescription = p.MetaDescription,
            BodyHtml = p.BodyHtml, IsPublished = p.IsPublished
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var p = await _db.ContentPages.FindAsync(Input.Id);
        if (p is null) return NotFound();
        Slug = p.Slug;
        if (!ModelState.IsValid) return Page();

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
