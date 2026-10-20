using GuysBee.Data;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Services.Shop;

public static class ShopKeys
{
    public const string Enabled = "shop.enabled";
    public const string AccountsEnabled = "shop.accounts";
    public const string HistoryEnabled = "shop.history";
    public const string RelayFinderUrl = "shop.relayFinderUrl";
    public const string DefaultRelayFinderUrl = "https://www.colissimo.fr/portail_colissimo/pointretrait";

    public static bool IsOn(IReadOnlyDictionary<string, string> settings, string key) =>
        settings.TryGetValue(key, out var v) && v == "1";
}

public record ShopConfig(bool Enabled, bool AccountsEnabled, bool HistoryEnabled, string RelayFinderUrl);

/// <summary>Lit les interrupteurs de la boutique (réglés dans l'administration).</summary>
public class ShopSettings
{
    private readonly AppDbContext _db;
    public ShopSettings(AppDbContext db) => _db = db;

    public async Task<ShopConfig> GetAsync()
    {
        var s = await _db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        var finder = s.TryGetValue(ShopKeys.RelayFinderUrl, out var u) && !string.IsNullOrWhiteSpace(u)
            ? u : ShopKeys.DefaultRelayFinderUrl;
        var enabled = ShopKeys.IsOn(s, ShopKeys.Enabled);
        return new ShopConfig(
            enabled,
            enabled && ShopKeys.IsOn(s, ShopKeys.AccountsEnabled),
            enabled && ShopKeys.IsOn(s, ShopKeys.AccountsEnabled) && ShopKeys.IsOn(s, ShopKeys.HistoryEnabled),
            finder);
    }
}
