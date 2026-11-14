using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Clients;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _users;

    public IndexModel(AppDbContext db, UserManager<IdentityUser> users)
    {
        _db = db;
        _users = users;
    }

    public bool ShopEnabled { get; private set; }
    public bool AccountsEnabled { get; private set; }
    public string? Q { get; private set; }
    public List<ClientRow> Clients { get; private set; } = new();

    public record ClientRow(string UserId, string Name, string Email, string? Phone, string? City, bool Blocked);

    public async Task OnGetAsync(string? q)
    {
        Q = q?.Trim();
        var s = await _db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        ShopEnabled = ShopKeys.IsOn(s, ShopKeys.Enabled);
        AccountsEnabled = ShopKeys.IsOn(s, ShopKeys.AccountsEnabled);

        var profiles = await _db.CustomerProfiles.AsNoTracking().ToListAsync();
        var ids = profiles.Select(p => p.UserId).ToList();
        var users = await _users.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
        var now = DateTimeOffset.UtcNow;

        Clients = profiles
            .Join(users, p => p.UserId, u => u.Id, (p, u) => new ClientRow(
                u.Id,
                $"{p.FirstName} {p.LastName}".Trim(),
                u.Email ?? "",
                p.Phone,
                p.City,
                u.LockoutEnd is { } end && end > now))
            .Where(c => string.IsNullOrEmpty(Q)
                || c.Name.Contains(Q, StringComparison.OrdinalIgnoreCase)
                || c.Email.Contains(Q, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Name)
            .ToList();
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
            : "Les inscriptions sont ouvertes. Pensez à activer aussi la commande en ligne dans « Boutique en ligne ».";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBlockAsync(string id, bool block)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null) return RedirectToPage();
        await _users.SetLockoutEnabledAsync(user, true);
        await _users.SetLockoutEndDateAsync(user, block ? DateTimeOffset.MaxValue : null);
        TempData["Message"] = block ? "Compte bloqué : le client ne peut plus se connecter." : "Compte réactivé.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null) return RedirectToPage();
        if (await _users.IsInRoleAsync(user, "Admin"))
        {
            TempData["Error"] = "Un compte administrateur ne peut pas être supprimé ici.";
            return RedirectToPage();
        }
        var profiles = await _db.CustomerProfiles.Where(p => p.UserId == id).ToListAsync();
        _db.CustomerProfiles.RemoveRange(profiles);
        await _db.SaveChangesAsync();
        await _users.DeleteAsync(user);
        TempData["Message"] = "Compte supprimé.";
        return RedirectToPage();
    }
}
