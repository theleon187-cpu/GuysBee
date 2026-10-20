using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuysBee.Areas.Admin.Pages.Integrations;

public class IndexModel : PageModel
{
    private readonly IntegrationSettings _settings;
    private readonly IntegrationConnector _connector;

    public IndexModel(IntegrationSettings settings, IntegrationConnector connector)
    {
        _settings = settings;
        _connector = connector;
    }

    [BindProperty] public PayPalSettings PayPal { get; set; } = new();
    [BindProperty] public InvoiceSettings Invoice { get; set; } = new();
    [BindProperty] public ColissimoSettings Colissimo { get; set; } = new();
    [BindProperty] public string? PayPalSecret { get; set; }
    [BindProperty] public string? ColissimoPassword { get; set; }

    public bool PayPalHasSecret { get; private set; }
    public bool ColissimoHasPassword { get; private set; }

    public async Task OnGetAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        PayPal = await _settings.GetPayPalAsync();
        Invoice = await _settings.GetInvoiceAsync();
        Colissimo = await _settings.GetColissimoAsync();
        PayPalHasSecret = !string.IsNullOrEmpty(PayPal.Secret);
        ColissimoHasPassword = !string.IsNullOrEmpty(Colissimo.Password);
        PayPal.Secret = "";
        Colissimo.Password = "";
    }

    public async Task<IActionResult> OnPostPayPalAsync()
    {
        await _settings.SavePayPalAsync(PayPal, PayPalSecret);
        TempData["Message"] = "Réglages PayPal enregistrés.";
        return RedirectToPage(null, null, "paypal");
    }

    public async Task<IActionResult> OnPostTestPayPalAsync()
    {
        await _settings.SavePayPalAsync(PayPal, PayPalSecret);
        var result = await _connector.TestPayPalAsync(await _settings.GetPayPalAsync());
        TempData[result.Ok ? "Message" : "Error"] = result.Message;
        return RedirectToPage(null, null, "paypal");
    }

    public async Task<IActionResult> OnPostClearPayPalSecretAsync()
    {
        await _settings.ClearPayPalSecretAsync();
        TempData["Message"] = "Clé secrète PayPal supprimée.";
        return RedirectToPage(null, null, "paypal");
    }

    public async Task<IActionResult> OnPostInvoiceAsync()
    {
        await _settings.SaveInvoiceAsync(Invoice);
        TempData["Message"] = "Informations de facturation enregistrées.";
        return RedirectToPage(null, null, "facturation");
    }

    public async Task<IActionResult> OnPostColissimoAsync()
    {
        await _settings.SaveColissimoAsync(Colissimo, ColissimoPassword);
        TempData["Message"] = "Réglages Colissimo et points relais enregistrés.";
        return RedirectToPage(null, null, "colissimo");
    }

    public async Task<IActionResult> OnPostTestColissimoAsync()
    {
        await _settings.SaveColissimoAsync(Colissimo, ColissimoPassword);
        var result = await _connector.TestColissimoAsync(await _settings.GetColissimoAsync());
        TempData[result.Ok ? "Message" : "Error"] = result.Message;
        return RedirectToPage(null, null, "colissimo");
    }

    public async Task<IActionResult> OnPostClearColissimoPasswordAsync()
    {
        await _settings.ClearColissimoPasswordAsync();
        TempData["Message"] = "Mot de passe Colissimo supprimé.";
        return RedirectToPage(null, null, "colissimo");
    }
}
