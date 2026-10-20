using System.Globalization;
using System.Security.Cryptography;
using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Services.Shop;

public class PayPalSettings
{
    public bool Enabled { get; set; }
    public bool IsLive { get; set; }
    public string ClientId { get; set; } = "";
    public string Secret { get; set; } = "";
    public string Currency { get; set; } = "EUR";
    public string BrandName { get; set; } = "GUY's Bee";
    public string PaymentDescription { get; set; } = "Commande GUY's Bee";

    public string ApiBaseUrl => IsLive ? "https://api-m.paypal.com" : "https://api-m.sandbox.paypal.com";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(Secret);
}

public class InvoiceSettings
{
    public string SellerName { get; set; } = "GUY's Bee";
    public string AddressLine { get; set; } = "";
    public string PostalCode { get; set; } = "69690";
    public string City { get; set; } = "Saint-Julien-sur-Bibost";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Siret { get; set; } = "";
    public string VatNumber { get; set; } = "";
    public bool VatExempt { get; set; } = true;
    public decimal VatRate { get; set; } = 5.5m;
    public string NumberPrefix { get; set; } = "FA";
    public string PaymentTerms { get; set; } = "Payée en ligne par PayPal.";
    public string FooterNote { get; set; } = "";
}

public class ColissimoSettings
{
    public bool Enabled { get; set; }
    public string ContractNumber { get; set; } = "";
    public string Password { get; set; } = "";
    public bool RelayWidgetEnabled { get; set; }
    public string SenderName { get; set; } = "GUY's Bee";
    public string SenderAddressLine { get; set; } = "";
    public string SenderPostalCode { get; set; } = "69690";
    public string SenderCity { get; set; } = "Saint-Julien-sur-Bibost";
    public int ParcelTareGrams { get; set; } = 250;
    public int MaxParcelGrams { get; set; } = 30000;
    public bool SignatureRequired { get; set; }
    public decimal? InsuranceAboveAmount { get; set; }
    public string RelayFinderUrl { get; set; } = ShopKeys.DefaultRelayFinderUrl;

    public bool HasCredentials => !string.IsNullOrWhiteSpace(ContractNumber) && !string.IsNullOrWhiteSpace(Password);
    public bool WidgetAvailable => Enabled && RelayWidgetEnabled && HasCredentials;
}

/// <summary>Lit et enregistre les réglages PayPal, facturation et Colissimo (secrets chiffrés).</summary>
public class IntegrationSettings
{
    private const string Encrypted = "enc:";
    private readonly AppDbContext _db;
    private readonly IDataProtector _protector;

    public IntegrationSettings(AppDbContext db, IDataProtectionProvider dataProtection)
    {
        _db = db;
        _protector = dataProtection.CreateProtector("GuysBee.Integrations.v1");
    }

    private Task<Dictionary<string, string>> LoadAsync() =>
        _db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);

    public async Task<PayPalSettings> GetPayPalAsync()
    {
        var d = await LoadAsync();
        var s = new PayPalSettings();
        return new PayPalSettings
        {
            Enabled = Bool(d, "paypal.enabled", s.Enabled),
            IsLive = Text(d, "paypal.mode", "sandbox") == "live",
            ClientId = Text(d, "paypal.clientId", s.ClientId),
            Secret = Secret(d, "paypal.secret"),
            Currency = Text(d, "paypal.currency", s.Currency),
            BrandName = Text(d, "paypal.brandName", s.BrandName),
            PaymentDescription = Text(d, "paypal.description", s.PaymentDescription),
        };
    }

    /// <summary>Si <paramref name="newSecret"/> est vide, le secret déjà enregistré est conservé.</summary>
    public async Task SavePayPalAsync(PayPalSettings s, string? newSecret)
    {
        var values = new Dictionary<string, string>
        {
            ["paypal.enabled"] = Flag(s.Enabled),
            ["paypal.mode"] = s.IsLive ? "live" : "sandbox",
            ["paypal.clientId"] = T(s.ClientId),
            ["paypal.currency"] = string.IsNullOrWhiteSpace(s.Currency) ? "EUR" : T(s.Currency).ToUpperInvariant(),
            ["paypal.brandName"] = T(s.BrandName),
            ["paypal.description"] = T(s.PaymentDescription),
        };
        if (!string.IsNullOrWhiteSpace(newSecret)) values["paypal.secret"] = Encrypted + _protector.Protect(newSecret.Trim());
        await UpsertAsync(values);
    }

    public async Task ClearPayPalSecretAsync() => await UpsertAsync(new() { ["paypal.secret"] = "" });

    public async Task<InvoiceSettings> GetInvoiceAsync()
    {
        var d = await LoadAsync();
        var s = new InvoiceSettings();
        return new InvoiceSettings
        {
            SellerName = Text(d, "invoice.sellerName", s.SellerName),
            AddressLine = Text(d, "invoice.address", s.AddressLine),
            PostalCode = Text(d, "invoice.postalCode", s.PostalCode),
            City = Text(d, "invoice.city", s.City),
            Email = Text(d, "invoice.email", s.Email),
            Phone = Text(d, "invoice.phone", s.Phone),
            Siret = Text(d, "invoice.siret", s.Siret),
            VatNumber = Text(d, "invoice.vatNumber", s.VatNumber),
            VatExempt = Bool(d, "invoice.vatExempt", s.VatExempt),
            VatRate = Decimal(d, "invoice.vatRate") ?? s.VatRate,
            NumberPrefix = Text(d, "invoice.prefix", s.NumberPrefix),
            PaymentTerms = Text(d, "invoice.paymentTerms", s.PaymentTerms),
            FooterNote = Text(d, "invoice.footer", s.FooterNote),
        };
    }

    public Task SaveInvoiceAsync(InvoiceSettings s) => UpsertAsync(new()
    {
        ["invoice.sellerName"] = T(s.SellerName),
        ["invoice.address"] = T(s.AddressLine),
        ["invoice.postalCode"] = T(s.PostalCode),
        ["invoice.city"] = T(s.City),
        ["invoice.email"] = T(s.Email),
        ["invoice.phone"] = T(s.Phone),
        ["invoice.siret"] = T(s.Siret),
        ["invoice.vatNumber"] = T(s.VatNumber),
        ["invoice.vatExempt"] = Flag(s.VatExempt),
        ["invoice.vatRate"] = Math.Clamp(s.VatRate, 0m, 100m).ToString(CultureInfo.InvariantCulture),
        ["invoice.prefix"] = T(s.NumberPrefix),
        ["invoice.paymentTerms"] = T(s.PaymentTerms),
        ["invoice.footer"] = T(s.FooterNote),
    });

    public async Task<ColissimoSettings> GetColissimoAsync()
    {
        var d = await LoadAsync();
        var s = new ColissimoSettings();
        return new ColissimoSettings
        {
            Enabled = Bool(d, "colissimo.enabled", s.Enabled),
            ContractNumber = Text(d, "colissimo.contract", s.ContractNumber),
            Password = Secret(d, "colissimo.password"),
            RelayWidgetEnabled = Bool(d, "colissimo.relayWidget", s.RelayWidgetEnabled),
            SenderName = Text(d, "colissimo.senderName", s.SenderName),
            SenderAddressLine = Text(d, "colissimo.senderAddress", s.SenderAddressLine),
            SenderPostalCode = Text(d, "colissimo.senderPostalCode", s.SenderPostalCode),
            SenderCity = Text(d, "colissimo.senderCity", s.SenderCity),
            ParcelTareGrams = (int?)Decimal(d, "colissimo.tare") ?? s.ParcelTareGrams,
            MaxParcelGrams = (int?)Decimal(d, "colissimo.maxWeight") ?? s.MaxParcelGrams,
            SignatureRequired = Bool(d, "colissimo.signature", s.SignatureRequired),
            InsuranceAboveAmount = Decimal(d, "colissimo.insuranceAbove"),
            RelayFinderUrl = Text(d, ShopKeys.RelayFinderUrl, s.RelayFinderUrl),
        };
    }

    /// <summary>Si <paramref name="newPassword"/> est vide, le mot de passe déjà enregistré est conservé.</summary>
    public async Task SaveColissimoAsync(ColissimoSettings s, string? newPassword)
    {
        var values = new Dictionary<string, string>
        {
            ["colissimo.enabled"] = Flag(s.Enabled),
            ["colissimo.contract"] = T(s.ContractNumber),
            ["colissimo.relayWidget"] = Flag(s.RelayWidgetEnabled),
            ["colissimo.senderName"] = T(s.SenderName),
            ["colissimo.senderAddress"] = T(s.SenderAddressLine),
            ["colissimo.senderPostalCode"] = T(s.SenderPostalCode),
            ["colissimo.senderCity"] = T(s.SenderCity),
            ["colissimo.tare"] = Math.Max(s.ParcelTareGrams, 0).ToString(CultureInfo.InvariantCulture),
            ["colissimo.maxWeight"] = Math.Max(s.MaxParcelGrams, 0).ToString(CultureInfo.InvariantCulture),
            ["colissimo.signature"] = Flag(s.SignatureRequired),
            ["colissimo.insuranceAbove"] = s.InsuranceAboveAmount is > 0 ? s.InsuranceAboveAmount.Value.ToString(CultureInfo.InvariantCulture) : "",
            [ShopKeys.RelayFinderUrl] = T(s.RelayFinderUrl) == ShopKeys.DefaultRelayFinderUrl ? "" : T(s.RelayFinderUrl),
        };
        if (!string.IsNullOrWhiteSpace(newPassword)) values["colissimo.password"] = Encrypted + _protector.Protect(newPassword.Trim());
        await UpsertAsync(values);
    }

    public async Task ClearColissimoPasswordAsync() => await UpsertAsync(new() { ["colissimo.password"] = "" });

    private async Task UpsertAsync(Dictionary<string, string> values)
    {
        var keys = values.Keys.ToList();
        var existing = await _db.SiteSettings.Where(x => keys.Contains(x.Key)).ToDictionaryAsync(x => x.Key);
        foreach (var (key, value) in values)
        {
            if (existing.TryGetValue(key, out var row)) row.Value = value;
            else _db.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
        }
        await _db.SaveChangesAsync();
    }

    private string Secret(Dictionary<string, string> d, string key)
    {
        var raw = Text(d, key, "");
        if (!raw.StartsWith(Encrypted, StringComparison.Ordinal)) return "";
        try { return _protector.Unprotect(raw[Encrypted.Length..]); }
        catch (CryptographicException) { return ""; }
    }

    private static string T(string? v) => v?.Trim() ?? "";
    private static string Flag(bool v) => v ? "1" : "0";
    private static bool Bool(Dictionary<string, string> d, string key, bool def) => d.TryGetValue(key, out var v) ? v == "1" : def;
    private static string Text(Dictionary<string, string> d, string key, string def) =>
        d.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : def;
    private static decimal? Decimal(Dictionary<string, string> d, string key) =>
        d.TryGetValue(key, out var v) && decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var r) ? r : null;
}
