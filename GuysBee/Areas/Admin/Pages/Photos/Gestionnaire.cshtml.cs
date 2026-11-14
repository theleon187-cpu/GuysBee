using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Photos;

public class GestionnaireModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly MediaService _media;

    public GestionnaireModel(AppDbContext db, MediaService media)
    {
        _db = db;
        _media = media;
    }

    public List<MediaAsset> Items { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Theme { get; set; }

    public string[] Themes => IndexModel.Themes;

    public async Task OnGetAsync()
    {
        var query = _db.MediaAssets.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(Theme)) query = query.Where(m => m.Theme == Theme);
        var list = await query.OrderBy(m => m.SortOrder).ThenBy(m => m.Id).ToListAsync();
        if (!string.IsNullOrWhiteSpace(Q))
        {
            var q = Q.Trim();
            list = list.Where(m =>
                (m.Caption?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                || (m.AltText?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                || m.SourceFileName.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        Items = list;
    }

    private IActionResult Back() => RedirectToPage(new { Q, Theme });

    public async Task<IActionResult> OnPostUploadAsync(List<IFormFile> files, int? maxWidth)
    {
        var added = 0;
        var errors = new List<string>();
        foreach (var f in files.Where(f => f.Length > 0))
        {
            try { if (await _media.AddUploadAsync(f, maxWidth) is not null) added++; }
            catch (InvalidOperationException ex) { errors.Add($"{f.FileName} : {ex.Message}"); }
            catch (Exception ex) when (ex is SixLabors.ImageSharp.ImageFormatException or SixLabors.ImageSharp.UnknownImageFormatException)
            {
                errors.Add($"{f.FileName} : ce fichier n'est pas une image lisible.");
            }
        }
        if (errors.Count > 0) TempData["Error"] = string.Join(" ", errors);
        TempData["Message"] = $"{added} photo(s) ajoutée(s) (en brouillon).";
        return Back();
    }

    public async Task<IActionResult> OnPostStatusAsync(int id, MediaStatus status)
    {
        var m = await _db.MediaAssets.FindAsync(id);
        if (m is null) return NotFound();
        m.Status = status;
        if (status != MediaStatus.Published) m.ShowInHero = false;
        await _db.SaveChangesAsync();
        TempData["Message"] = status switch
        {
            MediaStatus.Published => "Photo publiée sur le site.",
            MediaStatus.Hidden => "Photo masquée.",
            _ => "Photo remise en brouillon."
        };
        return Back();
    }

    public async Task<IActionResult> OnPostHeroAsync(int id)
    {
        var m = await _db.MediaAssets.FindAsync(id);
        if (m is null) return NotFound();
        if (m.ShowInHero)
        {
            m.ShowInHero = false;
            TempData["Message"] = "Photo retirée du carrousel.";
        }
        else
        {
            m.ShowInHero = true;
            m.Status = MediaStatus.Published;
            TempData["Message"] = "Photo ajoutée au carrousel de l'accueil.";
        }
        await _db.SaveChangesAsync();
        return Back();
    }

    public async Task<IActionResult> OnPostSaveAsync(int id, string? caption, string? alt, string? photoTheme, string? photoSeason)
    {
        var m = await _db.MediaAssets.FindAsync(id);
        if (m is null) return NotFound();
        m.Caption = caption?.Trim();
        m.AltText = alt?.Trim();
        m.Theme = photoTheme;
        m.Season = photoSeason;
        await _db.SaveChangesAsync();
        TempData["Message"] = "Photo enregistrée.";
        return Back();
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, int direction)
    {
        var all = await _db.MediaAssets.OrderBy(m => m.SortOrder).ThenBy(m => m.Id).ToListAsync();
        for (var i = 0; i < all.Count; i++) all[i].SortOrder = i;
        var idx = all.FindIndex(m => m.Id == id);
        var target = idx + (direction < 0 ? -1 : 1);
        if (idx >= 0 && target >= 0 && target < all.Count)
            (all[idx].SortOrder, all[target].SortOrder) = (all[target].SortOrder, all[idx].SortOrder);
        await _db.SaveChangesAsync();
        return Back();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _media.DeleteAsync(id);
        TempData["Message"] = "Photo supprimée.";
        return Back();
    }
}
