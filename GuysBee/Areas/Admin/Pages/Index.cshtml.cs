using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IntegrationSettings _integrations;
    public IndexModel(AppDbContext db, IntegrationSettings integrations)
    {
        _db = db;
        _integrations = integrations;
    }

    public int OrdersToPrepare { get; private set; }
    public int PendingOrders { get; private set; }
    public bool ShopEnabled { get; private set; }
    public int ActiveShippingMethods { get; private set; }
    public PayPalSettings PayPal { get; private set; } = new();
    public InvoiceSettings Invoice { get; private set; } = new();
    public ColissimoSettings Colissimo { get; private set; } = new();
    public List<Order> RecentOrders { get; private set; } = new();

    public bool PayPalReady => PayPal.Enabled && PayPal.IsConfigured;
    public bool InvoiceReady => !string.IsNullOrWhiteSpace(Invoice.AddressLine) && (Invoice.VatExempt || !string.IsNullOrWhiteSpace(Invoice.VatNumber)) && !string.IsNullOrWhiteSpace(Invoice.Siret);
    public bool ColissimoReady => Colissimo.Enabled && Colissimo.HasCredentials;
    public bool ShippingReady => ActiveShippingMethods > 0;

    public int UnreadMessages { get; private set; }
    public int DraftPhotos { get; private set; }
    public int PublishedPhotos { get; private set; }
    public int HeroPhotos { get; private set; }
    public int OutOfStock { get; private set; }
    public int ActiveVariants { get; private set; }
    public List<ContactMessage> RecentMessages { get; private set; } = new();

    public bool HasPhotos => PublishedPhotos > 0;
    public bool HasHero => HeroPhotos > 0;
    public bool StockFilled => ActiveVariants > 0 && OutOfStock == 0;

    public async Task OnGetAsync()
    {
        UnreadMessages = await _db.ContactMessages.CountAsync(m => !m.IsRead && !m.IsArchived);
        DraftPhotos = await _db.MediaAssets.CountAsync(m => m.Status == MediaStatus.Draft);
        PublishedPhotos = await _db.MediaAssets.CountAsync(m => m.Status == MediaStatus.Published);
        HeroPhotos = await _db.MediaAssets.CountAsync(m => m.Status == MediaStatus.Published && m.ShowInHero);
        OutOfStock = await _db.ProductVariants.CountAsync(v => v.IsActive && v.Stock <= 0);
        ActiveVariants = await _db.ProductVariants.CountAsync(v => v.IsActive);
        RecentMessages = await _db.ContactMessages.AsNoTracking()
            .Where(m => !m.IsArchived).OrderByDescending(m => m.CreatedUtc).Take(5).ToListAsync();

        OrdersToPrepare = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Preparing);
        PendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
        RecentOrders = await _db.Orders.AsNoTracking().OrderByDescending(o => o.CreatedUtc).Take(5).ToListAsync();
        ShopEnabled = (await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == ShopKeys.Enabled))?.Value == "1";
        ActiveShippingMethods = await _db.ShippingMethods.CountAsync(m => m.IsActive);
        PayPal = await _integrations.GetPayPalAsync();
        Invoice = await _integrations.GetInvoiceAsync();
        Colissimo = await _integrations.GetColissimoAsync();
    }
}
