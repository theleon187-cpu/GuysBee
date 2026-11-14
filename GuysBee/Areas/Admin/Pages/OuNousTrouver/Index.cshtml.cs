using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.OuNousTrouver;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public static readonly (string Value, string Label, string Help)[] Layouts =
    [
        ("liste", "Liste", "Une ligne par lieu, séparées par un fin trait (rendu actuel)."),
        ("cartes", "Cartes", "Chaque lieu dans sa propre carte, côte à côte."),
        ("compact", "Compact", "Lignes serrées, sans photo : idéal si vous avez beaucoup de lieux."),
    ];

    [BindProperty] public string Layout { get; set; } = "liste";
    [BindProperty] public string MapPosition { get; set; } = "dessus";

    public static readonly (string Value, string Label)[] MapPositions =
    [
        ("dessus", "Au-dessus de la liste"),
        ("cote", "À côté de la liste"),
        ("dessous", "Sous la liste"),
    ];
    [BindProperty] public bool ShowMap { get; set; } = true;
    [BindProperty] public bool ShowFilter { get; set; } = true;
    [BindProperty] public bool ShowPhotos { get; set; } = true;
    [BindProperty] public bool MarketsFirst { get; set; } = true;
    [BindProperty] public string MarketsTitle { get; set; } = "Sur les marchés";
    [BindProperty] public string ShopsTitle { get; set; } = "Dans les commerces";

    public async Task OnGetAsync()
    {
        var s = await _db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        string Get(string k, string d) => s.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v) ? v : d;
        Layout = Get("findus.layout", "liste");
        MapPosition = Get("findus.mapPosition", "dessus");
        ShowMap = Get("findus.showMap", "1") == "1";
        ShowFilter = Get("findus.showFilter", "1") == "1";
        ShowPhotos = Get("findus.showPhotos", "1") == "1";
        MarketsFirst = Get("findus.marketsFirst", "1") == "1";
        MarketsTitle = Get("findus.marketsTitle", MarketsTitle);
        ShopsTitle = Get("findus.shopsTitle", ShopsTitle);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var layout = Layouts.Any(l => l.Value == Layout) ? Layout : "liste";
        var values = new Dictionary<string, string>
        {
            ["findus.layout"] = layout,
            ["findus.mapPosition"] = MapPositions.Any(p => p.Value == MapPosition) ? MapPosition : "dessus",
            ["findus.showMap"] = ShowMap ? "1" : "0",
            ["findus.showFilter"] = ShowFilter ? "1" : "0",
            ["findus.showPhotos"] = ShowPhotos ? "1" : "0",
            ["findus.marketsFirst"] = MarketsFirst ? "1" : "0",
            ["findus.marketsTitle"] = (MarketsTitle ?? "").Trim(),
            ["findus.shopsTitle"] = (ShopsTitle ?? "").Trim(),
        };
        var existing = await _db.SiteSettings.Where(x => values.Keys.Contains(x.Key)).ToDictionaryAsync(x => x.Key);
        foreach (var (key, value) in values)
        {
            if (existing.TryGetValue(key, out var setting)) setting.Value = value;
            else _db.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
        }
        await _db.SaveChangesAsync();
        TempData["Message"] = "Affichage de la page « Où nous trouver » enregistré.";
        return RedirectToPage();
    }
}
