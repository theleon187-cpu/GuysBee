using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Historique;

public class IndexModel : PageModel
{
    private const int PageSize = 50;
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public string? Action { get; set; }
    [BindProperty(SupportsGet = true)] public string? Entity { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public List<AuditEntry> Entries { get; private set; } = new();
    public List<string> Entities { get; private set; } = new();
    public int TotalCount { get; private set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public DateTime WeekStartUtc { get; private set; }
    public int WeekCreated { get; private set; }
    public int WeekModified { get; private set; }
    public int WeekDeleted { get; private set; }

    public static readonly string[] Actions = { "Création", "Modification", "Suppression" };

    public static readonly (string Title, string Text)[] WeekSummary =
    {
        ("Contenu du site", "Textes, page d'accueil, prestations et photos modifiables depuis l'administration, avec mise à jour des images et des textes du site."),
        ("Navigation", "Menu du site à plusieurs niveaux (titres principaux et sous-liens) réorganisable depuis l'administration."),
        ("Agenda et points de vente", "Agenda des marchés (jours multiples, carte, coordonnées) et lieux de revente avec localisation sur carte."),
        ("Boutique", "Gammes et produits, commandes, paiement et livraison configurables (points relais, numéro de suivi), espace client avec profil et adresses de facturation."),
        ("Annonce", "Bandeau d'annonce du site : message, bouton, dates de début et de fin, fermeture mémorisée chez le visiteur."),
        ("Historique", "Nouvelle rubrique : chaque création, modification ou suppression faite en administration est désormais conservée avec la date et l'auteur.")
    };

    public async Task OnGetAsync()
    {
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
        return RedirectToPage();
    }
}
