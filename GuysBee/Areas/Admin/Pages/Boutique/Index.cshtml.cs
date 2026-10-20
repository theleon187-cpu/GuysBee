using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Boutique;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    [BindProperty] public bool ShopEnabled { get; set; }
    [BindProperty] public bool AccountsEnabled { get; set; }
    [BindProperty] public bool HistoryEnabled { get; set; }
    [BindProperty] public List<MethodInput> Methods { get; set; } = new();
    [BindProperty] public Dictionary<int, decimal?> ZonePrices { get; set; } = new();
    [BindProperty] public Dictionary<int, decimal?> RatePrices { get; set; } = new();

    public List<ShippingMethod> ShippingMethods { get; private set; } = new();

    public class MethodInput
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
        public string? CustomerMessage { get; set; }
        public decimal? FreeAboveAmount { get; set; }
        public int PreparationDays { get; set; }
    }

    private Task<List<ShippingMethod>> LoadMethodsAsync() =>
        _db.ShippingMethods.AsNoTracking().Include(m => m.Zones).ThenInclude(z => z.Rates)
            .OrderBy(m => m.SortOrder).ToListAsync();

    public async Task OnGetAsync()
    {
        var s = await _db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        ShopEnabled = ShopKeys.IsOn(s, ShopKeys.Enabled);
        AccountsEnabled = ShopKeys.IsOn(s, ShopKeys.AccountsEnabled);
        HistoryEnabled = ShopKeys.IsOn(s, ShopKeys.HistoryEnabled);
        ShippingMethods = await LoadMethodsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await SetAsync(ShopKeys.Enabled, ShopEnabled ? "1" : "0");
        await SetAsync(ShopKeys.AccountsEnabled, AccountsEnabled ? "1" : "0");
        await SetAsync(ShopKeys.HistoryEnabled, HistoryEnabled ? "1" : "0");

        var methods = await _db.ShippingMethods.Include(m => m.Zones).ThenInclude(z => z.Rates).ToListAsync();
        foreach (var input in Methods)
        {
            var m = methods.FirstOrDefault(x => x.Id == input.Id);
            if (m is null) continue;
            if (!string.IsNullOrWhiteSpace(input.Name)) m.Name = input.Name.Trim();
            m.IsActive = input.IsActive;
            m.CustomerMessage = input.CustomerMessage?.Trim();
            m.FreeAboveAmount = input.FreeAboveAmount is > 0 ? input.FreeAboveAmount : null;
            m.PreparationDays = Math.Max(input.PreparationDays, 0);
            foreach (var z in m.Zones)
            {
                if (ZonePrices.TryGetValue(z.Id, out var zp) && zp is >= 0) z.FlatPrice = zp;
                foreach (var r in z.Rates)
                    if (RatePrices.TryGetValue(r.Id, out var rp) && rp is >= 0) r.Price = rp.Value;
            }
        }
        await _db.SaveChangesAsync();
        TempData["Message"] = "Param\u00e8tres de la boutique enregistr\u00e9s.";
        return RedirectToPage();
    }

    private async Task SetAsync(string key, string value)
    {
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null) _db.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
        else setting.Value = value;
    }
}
