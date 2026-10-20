using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services.Shipping;
using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages.Commande;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly CartService _cart;
    private readonly ShopSettings _shop;
    private readonly IntegrationSettings _integrations;
    private readonly IntegrationConnector _connector;

    public IndexModel(AppDbContext db, CartService cart, ShopSettings shop, IntegrationSettings integrations, IntegrationConnector connector)
    {
        _db = db;
        _cart = cart;
        _shop = shop;
        _integrations = integrations;
        _connector = connector;
    }

    public ColissimoSettings Colissimo { get; private set; } = new();

    [BindProperty] public InputModel Input { get; set; } = new();

    public ShopConfig Config { get; private set; } = new(false, false, false, "");
    public List<CartItem> Items { get; private set; } = new();
    public IReadOnlyList<ShippingQuote> Quotes { get; private set; } = Array.Empty<ShippingQuote>();
    public Dictionary<int, ShippingKind> Kinds { get; private set; } = new();
    public decimal ItemsTotal => Items.Sum(i => i.LineTotal);
    public bool IsSignedIn => User.Identity?.IsAuthenticated == true;

    public class InputModel
    {
        [Required(ErrorMessage = "Merci d'indiquer votre e-mail."), EmailAddress(ErrorMessage = "E-mail invalide.")]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "Merci d'indiquer votre prénom.")] public string FirstName { get; set; } = "";
        [Required(ErrorMessage = "Merci d'indiquer votre nom.")] public string LastName { get; set; } = "";
        public string? Phone { get; set; }
        [Required(ErrorMessage = "Merci d'indiquer votre adresse.")] public string AddressLine { get; set; } = "";
        [Required(ErrorMessage = "Merci d'indiquer le code postal.")] public string PostalCode { get; set; } = "";
        [Required(ErrorMessage = "Merci d'indiquer la ville.")] public string City { get; set; } = "";
        public bool BillingSameAsDelivery { get; set; } = true;
        public string? BillingName { get; set; }
        public string? BillingAddressLine { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCity { get; set; }
        public int? ShippingMethodId { get; set; }
        public string? RelayName { get; set; }
        public string? RelayAddress { get; set; }
        public string? Note { get; set; }
        public bool SaveToProfile { get; set; } = true;
        [Range(typeof(bool), "true", "true", ErrorMessage = "Merci d'accepter les conditions générales de vente.")]
        public bool AcceptCgv { get; set; }
    }

    private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private async Task<IActionResult?> PrepareAsync()
    {
        Config = await _shop.GetAsync();
        if (!Config.Enabled) return NotFound();
        if (Config.AccountsEnabled && !IsSignedIn)
            return Redirect("/Compte/Connexion?returnUrl=%2FCommande");
        Items = await _cart.LoadAsync();
        if (Items.Count == 0) return RedirectToPage("/Panier");
        return null;
    }

    private async Task LoadQuotesAsync()
    {
        var methods = await _db.ShippingMethods.AsNoTracking()
            .Include(m => m.Zones).ThenInclude(z => z.Rates)
            .Where(m => m.IsActive).ToListAsync();
        Kinds = methods.ToDictionary(m => m.Id, m => m.Kind);
        var lines = Items.Select(i => new CartLine(i.WeightGrams, i.Quantity, i.UnitPrice)).ToList();
        Colissimo = await _integrations.GetColissimoAsync();
        if (Colissimo.Enabled && Colissimo.ParcelTareGrams > 0) lines.Add(new CartLine(Colissimo.ParcelTareGrams, 1, 0m));
        var quotes = ShippingCalculator.Quote(methods, lines, Input.PostalCode);
        var totalWeight = lines.Sum(l => l.WeightGrams * l.Quantity);
        if (Colissimo.Enabled && Colissimo.MaxParcelGrams > 0 && totalWeight > Colissimo.MaxParcelGrams)
            quotes = quotes.Where(q => Kinds[q.MethodId] is not (ShippingKind.Parcel or ShippingKind.Relay)).ToList();
        Quotes = quotes;
    }

    public async Task<IActionResult> OnGetRelayTokenAsync()
    {
        Config = await _shop.GetAsync();
        if (!Config.Enabled) return NotFound();
        var settings = await _integrations.GetColissimoAsync();
        if (!settings.WidgetAvailable) return NotFound();
        var (token, _) = await _connector.GetColissimoTokenAsync(settings, HttpContext.RequestAborted);
        return token is null ? StatusCode(StatusCodes.Status502BadGateway) : new JsonResult(new { token });
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (await PrepareAsync() is { } early) return early;

        if (IsSignedIn)
        {
            var uid = UserId;
            var profile = await _db.CustomerProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == uid);
            Input.Email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "";
            if (profile is not null)
            {
                Input.FirstName = profile.FirstName;
                Input.LastName = profile.LastName;
                Input.Phone = profile.Phone;
                Input.AddressLine = profile.AddressLine ?? "";
                Input.PostalCode = profile.PostalCode ?? "";
                Input.City = profile.City ?? "";
                Input.BillingSameAsDelivery = profile.BillingSameAsDelivery;
                Input.BillingName = profile.BillingName;
                Input.BillingAddressLine = profile.BillingAddressLine;
                Input.BillingPostalCode = profile.BillingPostalCode;
                Input.BillingCity = profile.BillingCity;
            }
        }
        ModelState.Clear();
        await LoadQuotesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRefreshAsync()
    {
        if (await PrepareAsync() is { } early) return early;
        ModelState.Clear();
        await LoadQuotesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (await PrepareAsync() is { } early) return early;
        await LoadQuotesAsync();

        var quote = Quotes.FirstOrDefault(q => q.MethodId == Input.ShippingMethodId);
        if (quote is null)
            ModelState.AddModelError("Input.ShippingMethodId", "Merci de choisir un mode de livraison.");
        else if (Kinds.TryGetValue(quote.MethodId, out var kind) && kind == ShippingKind.Relay
                 && (string.IsNullOrWhiteSpace(Input.RelayName) || string.IsNullOrWhiteSpace(Input.RelayAddress)))
            ModelState.AddModelError("Input.RelayName", "Merci d'indiquer le nom et l'adresse du point relais choisi.");
        if (!Input.BillingSameAsDelivery
            && (string.IsNullOrWhiteSpace(Input.BillingName) || string.IsNullOrWhiteSpace(Input.BillingAddressLine)
                || string.IsNullOrWhiteSpace(Input.BillingPostalCode) || string.IsNullOrWhiteSpace(Input.BillingCity)))
            ModelState.AddModelError("Input.BillingName", "Merci de compléter l'adresse de facturation.");
        if (!ModelState.IsValid) return Page();

        var ids = Items.Select(i => i.VariantId).ToList();
        var variants = await _db.ProductVariants.Where(v => ids.Contains(v.Id)).ToDictionaryAsync(v => v.Id);
        var order = new Order
        {
            Number = await NewNumberAsync(),
            Status = OrderStatus.Pending,
            UserId = IsSignedIn ? UserId : null,
            CustomerName = $"{Input.FirstName.Trim()} {Input.LastName.Trim()}",
            CustomerEmail = Input.Email.Trim(),
            CustomerPhone = Input.Phone?.Trim(),
            AddressLine = Input.AddressLine.Trim(),
            PostalCode = Input.PostalCode.Trim(),
            City = Input.City.Trim(),
            BillingName = Input.BillingSameAsDelivery ? $"{Input.FirstName.Trim()} {Input.LastName.Trim()}" : Input.BillingName!.Trim(),
            BillingAddressLine = Input.BillingSameAsDelivery ? Input.AddressLine.Trim() : Input.BillingAddressLine!.Trim(),
            BillingPostalCode = Input.BillingSameAsDelivery ? Input.PostalCode.Trim() : Input.BillingPostalCode!.Trim(),
            BillingCity = Input.BillingSameAsDelivery ? Input.City.Trim() : Input.BillingCity!.Trim(),
            ShippingMethodId = quote!.MethodId,
            ShippingMethodLabel = quote.MethodName,
            RelayPointLabel = Kinds[quote.MethodId] == ShippingKind.Relay ? $"{Input.RelayName!.Trim()} - {Input.RelayAddress!.Trim()}" : null,
            CustomerNote = Input.Note?.Trim(),
            ItemsTotal = ItemsTotal,
            ShippingCost = quote.Price,
            Total = ItemsTotal + quote.Price,
        };
        foreach (var i in Items)
        {
            if (!variants.TryGetValue(i.VariantId, out var v) || v.Stock < i.Quantity)
            {
                ModelState.AddModelError("", "Le stock a changé : merci de vérifier votre panier.");
                return Page();
            }
            v.Stock -= i.Quantity;
            order.Lines.Add(new OrderLine
            {
                ProductVariantId = i.VariantId,
                Label = i.Label,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                WeightGrams = i.WeightGrams,
            });
        }
        _db.Orders.Add(order);

        if (IsSignedIn && Input.SaveToProfile) await SaveProfileAsync();
        await _db.SaveChangesAsync();

        _cart.Clear();
        TempData["OrderNumber"] = order.Number;
        return Redirect($"/Commande/Confirmation/{order.Number}");
    }

    private async Task SaveProfileAsync()
    {
        var uid = UserId!;
        var profile = await _db.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == uid);
        if (profile is null)
        {
            profile = new CustomerProfile { UserId = uid };
            _db.CustomerProfiles.Add(profile);
        }
        profile.FirstName = Input.FirstName.Trim();
        profile.LastName = Input.LastName.Trim();
        profile.Phone = Input.Phone?.Trim();
        profile.AddressLine = Input.AddressLine.Trim();
        profile.PostalCode = Input.PostalCode.Trim();
        profile.City = Input.City.Trim();
        profile.BillingSameAsDelivery = Input.BillingSameAsDelivery;
        profile.BillingName = Input.BillingSameAsDelivery ? null : Input.BillingName?.Trim();
        profile.BillingAddressLine = Input.BillingSameAsDelivery ? null : Input.BillingAddressLine?.Trim();
        profile.BillingPostalCode = Input.BillingSameAsDelivery ? null : Input.BillingPostalCode?.Trim();
        profile.BillingCity = Input.BillingSameAsDelivery ? null : Input.BillingCity?.Trim();
        profile.UpdatedUtc = DateTime.UtcNow;
    }

    private async Task<string> NewNumberAsync()
    {
        while (true)
        {
            var number = $"GB-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 10000)}";
            if (!await _db.Orders.AnyAsync(o => o.Number == number)) return number;
        }
    }
}
