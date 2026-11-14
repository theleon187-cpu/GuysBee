using System.Net;
using System.Text.RegularExpressions;
using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Prestations;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly MediaService _media;
    private readonly IHtmlCleaner _cleaner;

    public IndexModel(AppDbContext db, MediaService media, IHtmlCleaner cleaner)
    {
        _db = db;
        _media = media;
        _cleaner = cleaner;
    }

    public ContentPage? PageData { get; private set; }
    public string IntroText { get; private set; } = "";
    public List<Prestation> Items { get; private set; } = new();

    public async Task OnGetAsync()
    {
        PageData = await _db.ContentPages.AsNoTracking().Include(p => p.HeroMedia).FirstOrDefaultAsync(p => p.Key == "prestations");
        IntroText = PageData?.BodyHtml ?? string.Empty;
        Items = await _db.Prestations.AsNoTracking().Include(p => p.Media).OrderBy(p => p.SortOrder).ThenBy(p => p.Id).ToListAsync();
    }

    public async Task<IActionResult> OnPostSavePageAsync(string title, string? lead, string? intro, bool isPublished,
        IFormFile? heroPhoto, bool removeHero)
    {
        var page = await _db.ContentPages.FirstOrDefaultAsync(p => p.Key == "prestations");
        if (page is null) return NotFound();
        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["Error"] = "Le titre de la page est obligatoire.";
            return RedirectToPage();
        }
        page.Title = title.Trim();
        page.MetaDescription = lead?.Trim();
        page.BodyHtml = _cleaner.Clean(intro);
        page.IsPublished = isPublished;
        page.UpdatedUtc = DateTime.UtcNow;
        if (removeHero) page.HeroMediaId = null;
        if (!await TryAttachPhotoAsync(heroPhoto, id => page.HeroMediaId = id)) return RedirectToPage();
        await _db.SaveChangesAsync();
        TempData["Message"] = "Présentation de la page enregistrée.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var next = (await _db.Prestations.MaxAsync(p => (int?)p.SortOrder) ?? 0) + 1;
        _db.Prestations.Add(new Prestation { Title = "Nouvelle prestation", Description = "Décrivez ici cette prestation.", PriceInfo = "Sur devis", SortOrder = next });
        await _db.SaveChangesAsync();
        TempData["Message"] = "Prestation ajoutée : modifiez son titre et son texte, puis enregistrez.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveAsync(int id, string title, string? description, string? priceInfo,
        bool isVisible, IFormFile? photo, bool removePhoto)
    {
        var p = await _db.Prestations.FindAsync(id);
        if (p is null) return NotFound();
        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["Error"] = "Le titre est obligatoire.";
            return RedirectToPage();
        }
        p.Title = title.Trim();
        p.Description = _cleaner.Clean(description);
        p.PriceInfo = string.IsNullOrWhiteSpace(priceInfo) ? null : priceInfo.Trim();
        p.IsVisible = isVisible;
        if (removePhoto) p.MediaId = null;
        if (!await TryAttachPhotoAsync(photo, mid => p.MediaId = mid)) return RedirectToPage();
        await _db.SaveChangesAsync();
        TempData["Message"] = $"« {p.Title} » enregistrée.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, int direction)
    {
        var all = await _db.Prestations.OrderBy(p => p.SortOrder).ThenBy(p => p.Id).ToListAsync();
        var index = all.FindIndex(p => p.Id == id);
        var target = index + (direction < 0 ? -1 : 1);
        if (index >= 0 && target >= 0 && target < all.Count)
            (all[index], all[target]) = (all[target], all[index]);
        for (var i = 0; i < all.Count; i++) all[i].SortOrder = i + 1;
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var p = await _db.Prestations.FindAsync(id);
        if (p is not null)
        {
            _db.Prestations.Remove(p);
            await _db.SaveChangesAsync();
            TempData["Message"] = $"« {p.Title} » supprimée.";
        }
        return RedirectToPage();
    }

    private async Task<bool> TryAttachPhotoAsync(IFormFile? file, Action<int> assign)
    {
        if (file is not { Length: > 0 }) return true;
        try
        {
            var baseName = await _media.SavePhotoAsync(file);
            var asset = baseName is null ? null : await _db.MediaAssets.FirstOrDefaultAsync(m => m.BaseName == baseName);
            if (asset is not null) assign(asset.Id);
            return true;
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return false;
        }
    }

    private string ToHtml(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var paragraphs = Regex.Split(text.Trim(), @"(?:\r?\n){2,}|\r?\n")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => "<p>" + WebUtility.HtmlEncode(s.Trim()) + "</p>");
        return _cleaner.Clean(string.Concat(paragraphs)) ?? string.Empty;
    }

    private static string ToPlainText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var text = Regex.Replace(html, @"</p>\s*", "\n\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
        return WebUtility.HtmlDecode(Regex.Replace(text, "<[^>]+>", "")).Trim();
    }
}
