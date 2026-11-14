using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Historique;

public class IndexModel : PageModel
{
    private const int PageSize = 50;
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "evolutions";
    [BindProperty(SupportsGet = true)] public string? Action { get; set; }
    [BindProperty(SupportsGet = true)] public string? Entity { get; set; }
    [BindProperty(SupportsGet = true)] public string? Category { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public bool ShowJournal => Tab == "journal";
    public IReadOnlyList<ChangeRelease> Releases { get; private set; } = Array.Empty<ChangeRelease>();
    public int ItemCount => Releases.Sum(r => r.Items.Count);

    public List<AuditEntry> Entries { get; private set; } = new();
    public List<string> Entities { get; private set; } = new();
    public int TotalCount { get; private set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public DateTime WeekStartUtc { get; private set; }
    public int WeekCreated { get; private set; }
    public int WeekModified { get; private set; }
    public int WeekDeleted { get; private set; }

    public static readonly string[] Actions = { "Création", "Modification", "Suppression" };

    public static string TagClass(string category) => category switch
    {
        ChangeLog.Public => "gb-tag-public",
        ChangeLog.Admin => "gb-tag-admin",
        ChangeLog.Shop => "gb-tag-boutique",
        _ => "gb-tag-technique"
    };

    public static string ActionClass(string action) => action switch
    {
        "Création" => "gb-hist-action-creation",
        "Suppression" => "gb-hist-action-suppression",
        _ => "gb-hist-action-modification"
    };

    public async Task OnGetAsync()
    {
        Tab = Tab == "journal" ? "journal" : "evolutions";

        Releases = ChangeLog.Releases
            .Select(r => string.IsNullOrWhiteSpace(Category)
                ? r
                : r with { Items = r.Items.Where(i => i.Category == Category).ToList() })
            .Where(r => r.Items.Count > 0)
            .ToList();

        if (!ShowJournal) return;

        var today = DateTime.UtcNow.Date;
        WeekStartUtc = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));

        var week = await _db.AuditEntries.AsNoTracking()
            .Where(a => a.CreatedUtc >= WeekStartUtc)
            .GroupBy(a => a.Action)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();
        WeekCreated = week.FirstOrDefault(w => w.Key == "Création")?.Count ?? 0;
        WeekModified = week.FirstOrDefault(w => w.Key == "Modification")?.Count ?? 0;
        WeekDeleted = week.FirstOrDefault(w => w.Key == "Suppression")?.Count ?? 0;

        Entities = await _db.AuditEntries.AsNoTracking().Select(a => a.EntityName).Distinct().OrderBy(n => n).ToListAsync();

        var query = _db.AuditEntries.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(Action)) query = query.Where(a => a.Action == Action);
        if (!string.IsNullOrWhiteSpace(Entity)) query = query.Where(a => a.EntityName == Entity);

        TotalCount = await query.CountAsync();
        PageNumber = Math.Clamp(PageNumber, 1, TotalPages);
        Entries = await query.OrderByDescending(a => a.CreatedUtc).ThenByDescending(a => a.Id)
            .Skip((PageNumber - 1) * PageSize).Take(PageSize).ToListAsync();
    }

    public async Task<IActionResult> OnPostPurgeAsync()
    {
        var limit = DateTime.UtcNow.AddDays(-365);
        var removed = await _db.AuditEntries.Where(a => a.CreatedUtc < limit).ExecuteDeleteAsync();
        TempData["Message"] = removed == 0
            ? "Aucune ligne de plus d'un an à supprimer."
            : $"{removed} ancienne(s) ligne(s) supprimée(s).";
        return RedirectToPage(new { Tab = "journal" });
    }
}
