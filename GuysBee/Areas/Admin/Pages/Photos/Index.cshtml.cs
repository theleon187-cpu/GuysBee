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

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)] public string? Filter { get; set; }
    public int CountAll { get; private set; }    public int CountDraft { get; private set; }
    public int CountPublished { get; private set; }
    public int CountHidden { get; private set; }
    public int CountNoAlt { get; private set; }

    public async Task OnGetAsync()
    {
        var all = await _db.MediaAssets.AsNoTracking().OrderBy(m => m.SortOrder).ToListAsync();
        CountAll = all.Count;
        CountDraft = all.Count(m => m.Status == MediaStatus.Draft);
        CountPublished = all.Count(m => m.Status == MediaStatus.Published);
        CountHidden = all.Count(m => m.Status == MediaStatus.Hidden);
        CountNoAlt = all.Count(m => string.IsNullOrWhiteSpace(m.AltText));
        Items = Filter switch
        {
            "brouillon" => all.Where(m => m.Status == MediaStatus.Draft).ToList(),
            "publiee" => all.Where(m => m.Status == MediaStatus.Published).ToList(),
            "masquee" => all.Where(m => m.Status == MediaStatus.Hidden).ToList(),
            "sansdescription" => all.Where(m => string.IsNullOrWhiteSpace(m.AltText)).ToList(),
            _ => all
        };
    }

    public async Task<IActionResult> OnPostBulkAsync(List<int> ids, string? action, string? theme, string? season, string? filter)
    {
        if (ids is null || ids.Count == 0)
        {
            TempData["Error"] = "Cochez d'abord une ou plusieurs photos.";
            return RedirectToPage(new { Filter = filter });
        }
        if (action == "supprimer")
        {
            foreach (var id in ids) await _media.DeleteAsync(id);
            TempData["Message"] = $"{ids.Count} photo(s) supprimée(s).";
            return RedirectToPage(new { Filter = filter });
        }
        var items = await _db.MediaAssets.Where(m => ids.Contains(m.Id)).ToListAsync();
        foreach (var m in items)
        {
            switch (action)
            {
                case "publier": m.Status = MediaStatus.Published; break;
                case "masquer": m.Status = MediaStatus.Hidden; m.ShowInHero = false; break;
                case "brouillon": m.Status = MediaStatus.Draft; m.ShowInHero = false; break;
                case "classer":
                    if (!string.IsNullOrEmpty(theme)) m.Theme = theme;
                    if (!string.IsNullOrEmpty(season)) m.Season = season;
                    break;
            }
        }
        await _db.SaveChangesAsync();
        TempData["Message"] = $"{items.Count} photo(s) mise(s) à jour.";
        return RedirectToPage(new { Filter = filter });
    }

    public async Task<IActionResult> OnPostImportAsync(int? maxWidth)
    {
        var r = await _media.ImportFolderAsync(maxWidth);
        TempData["Message"] = $"Import terminé : {r.Imported} photo(s) ajoutée(s), {r.Duplicates} doublon(s) ignoré(s), {r.Errors} erreur(s).";
        return RedirectToPage();
    }

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
        TempData["Message"] = $"{added} photo(s) ajoutée(s).";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleHeroAsync(int id)
    {
        var m = await _db.MediaAssets.FindAsync(id);
        if (m is null) return NotFound();
        if (!m.ShowInHero)
        {
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
