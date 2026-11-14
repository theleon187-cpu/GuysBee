using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Clients;

public class InscriptionsModel : PageModel
{
    private readonly AppDbContext _db;
    public InscriptionsModel(AppDbContext db) => _db = db;

    public bool ShopEnabled { get; private set; }
    public bool AccountsEnabled { get; private set; }
    public int ClientCount { get; private set; }

    public async Task OnGetAsync()
    {
        var s = await _db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        ShopEnabled = ShopKeys.IsOn(s, ShopKeys.Enabled);
        AccountsEnabled = ShopKeys.IsOn(s, ShopKeys.AccountsEnabled);
        ClientCount = await _db.CustomerProfiles.CountAsync();
    }

    public async Task<IActionResult> OnPostToggleAsync()
    {
        var key = ShopKeys.AccountsEnabled;
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Key == key);
        var enabled = setting is not null && ShopKeys.IsOn(new Dictionary<string, string> { [key] = setting.Value }, key);
        var value = enabled ? "0" : "1";
        if (setting is null) _db.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
        else setting.Value = value;
        await _db.SaveChangesAsync();
        TempData["Message"] = enabled
            ? "Les inscriptions sont fermées : les clients commandent sans compte."
            : "Les inscriptions sont ouvertes.";
        return RedirectToPage();
    }
}
