using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Photos;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly MediaService _media;

    public IndexModel(AppDbContext db, MediaService media)
    {
        _db = db;
        _media = media;
    }

    public List<MediaAsset> Items { get; private set; } = new();
    public string SourceFolder => _media.SourceFolder;

    public static readonly string[] Themes =
        ["", "Ruches", "Abeilles", "Récolte", "Pots de miel", "Paysage et fleurs", "Matériel", "Apiculteur", "Marché et événements", "Autre"];
    public static readonly string[] Seasons = ["", "Printemps", "Été", "Automne", "Hiver"];

    public async Task OnGetAsync() =>
        Items = await _db.MediaAssets.AsNoTracking().OrderBy(m => m.SortOrder).ToListAsync();

    public async Task<IActionResult> OnPostImportAsync()
    {
        var r = await _media.ImportFolderAsync();
        TempData["Message"] = $"Import terminé : {r.Imported} photo(s) ajoutée(s), {r.Duplicates} doublon(s) ignoré(s), {r.Errors} erreur(s).";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUploadAsync(List<IFormFile> files)
    {
        var added = 0;
        var errors = new List<string>();
        foreach (var f in files.Where(f => f.Length > 0))
        {
            try { if (await _media.AddUploadAsync(f) is not null) added++; }
            catch (InvalidOperationException ex) { errors.Add($"{f.FileName} : {ex.Message}"); }
            catch (Exception ex) when (ex is SixLabors.ImageSharp.ImageFormatException or SixLabors.ImageSharp.UnknownImageFormatException)
            {
                errors.Add($"{f.FileName} : ce fichier n'est pas une image lisible.");
            }
        }
        if (errors.Count > 0) TempData["Error"] = string.Join(" ", errors);
        TempData["Message"] = $"{added} photo(s) ajoutée(s).";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleHeroAsync(int id)
    {
        var m = await _db.MediaAssets.FindAsync(id);
        if (m is null) return NotFound();
        if (!m.ShowInHero)
        {
            var count = await _db.MediaAssets.CountAsync(x => x.ShowInHero && x.Status == MediaStatus.Published);
            if (count >= 5)
            {
                TempData["Error"] = "Le carrousel est limité à 5 photos. Retirez-en une avant d'en ajouter.";
                return RedirectToPage();
            }
            m.ShowInHero = true;
            m.Status = MediaStatus.Published;
            TempData["Message"] = "Photo ajoutée au carrousel de l'accueil.";
        }
        else
        {
            m.ShowInHero = false;
            TempData["Message"] = "Photo retirée du carrousel.";
        }
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveAsync(int id, string? caption, string? alt, string? theme, string? season, MediaStatus status, bool showInHero)
    {
        var m = await _db.MediaAssets.FindAsync(id);
        if (m is null) return NotFound();
        if (showInHero && !m.ShowInHero
            && await _db.MediaAssets.CountAsync(x => x.ShowInHero && x.Status == MediaStatus.Published) >= 5)
        {
            TempData["Error"] = "Le carrousel est limité à 5 photos. Retirez-en une avant d'en ajouter.";
            return RedirectToPage();
        }
        m.Caption = caption?.Trim();
        m.AltText = alt?.Trim();
        m.Theme = theme;
        m.Season = season;
        m.Status = status;
        m.ShowInHero = showInHero;
        await _db.SaveChangesAsync();
        TempData["Message"] = "Photo enregistrée.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _media.DeleteAsync(id);
        TempData["Message"] = "Photo supprimée.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostPublishAllAsync()
    {
        var drafts = await _db.MediaAssets.Where(m => m.Status == MediaStatus.Draft).ToListAsync();
        foreach (var m in drafts) m.Status = MediaStatus.Published;
        await _db.SaveChangesAsync();
        TempData["Message"] = $"{drafts.Count} photo(s) publiée(s).";
        return RedirectToPage();
    }
}
