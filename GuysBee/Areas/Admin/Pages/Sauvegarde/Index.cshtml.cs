using GuysBee.Data;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Sauvegarde;

[RequestSizeLimit(1_073_741_824)]
[RequestFormLimits(MultipartBodyLengthLimit = 1_073_741_824)]
public class IndexModel : PageModel
{
    private readonly SiteBackupService _backup;    private readonly AppDbContext _db;

    public IndexModel(SiteBackupService backup, AppDbContext db)
    {
        _backup = backup;
        _db = db;
    }

    public DateTime? LastExport { get; private set; }

    public async Task OnGetAsync()
    {
        var value = (await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == SiteBackupService.LastExportKey))?.Value;
        if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var d))
            LastExport = d.ToLocalTime();
    }

    public async Task<IActionResult> OnPostExportAsync()
    {
        var stream = await _backup.ExportAsync();
        return File(stream, "application/zip", $"sauvegarde-guysbee-{DateTime.Now:yyyy-MM-dd-HHmm}.zip");
    }

    public async Task<IActionResult> OnPostImportAsync(IFormFile? archive, bool confirm)
    {
        if (archive is null || archive.Length == 0)
        {
            TempData["Error"] = "Choisissez d'abord un fichier de sauvegarde.";
            return RedirectToPage();
        }
        if (!confirm)
        {
            TempData["Error"] = "Cochez la case de confirmation pour restaurer la sauvegarde.";
            return RedirectToPage();
        }

        var temp = Path.Combine(Path.GetTempPath(), $"guysbee-import-{Guid.NewGuid():N}.zip");
        try
        {
            await using (var fs = new FileStream(temp, FileMode.Create))
                await archive.CopyToAsync(fs);
            await using var input = new FileStream(temp, FileMode.Open, FileAccess.Read);
            var error = await _backup.ImportAsync(input);
            if (error is not null) TempData["Error"] = error;
            else TempData["Message"] = "Sauvegarde restaur\u00e9e : textes, produits, photos et r\u00e9glages ont \u00e9t\u00e9 remplac\u00e9s.";
        }
        finally
        {
            if (System.IO.File.Exists(temp)) System.IO.File.Delete(temp);
        }
        return RedirectToPage();
    }
}
