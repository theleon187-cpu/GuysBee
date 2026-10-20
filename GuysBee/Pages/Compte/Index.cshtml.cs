using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages.Compte;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ShopSettings _shop;

    public IndexModel(AppDbContext db, ShopSettings shop)
    {
        _db = db;
        _shop = shop;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    public bool HistoryEnabled { get; private set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Merci d'indiquer votre prénom.")] public string FirstName { get; set; } = "";
        [Required(ErrorMessage = "Merci d'indiquer votre nom.")] public string LastName { get; set; } = "";
        public string? Phone { get; set; }
        public string? AddressLine { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public bool BillingSameAsDelivery { get; set; } = true;
        public string? BillingName { get; set; }
        public string? BillingAddressLine { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCity { get; set; }
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> OnGetAsync()
    {
        var config = await _shop.GetAsync();
        if (!config.AccountsEnabled) return NotFound();
        HistoryEnabled = config.HistoryEnabled;

        var p = await _db.CustomerProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == UserId);
        if (p is not null)
            Input = new InputModel
            {
                FirstName = p.FirstName, LastName = p.LastName, Phone = p.Phone,
                AddressLine = p.AddressLine, PostalCode = p.PostalCode, City = p.City,
                BillingSameAsDelivery = p.BillingSameAsDelivery, BillingName = p.BillingName,
                BillingAddressLine = p.BillingAddressLine, BillingPostalCode = p.BillingPostalCode, BillingCity = p.BillingCity,
            };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var config = await _shop.GetAsync();
        if (!config.AccountsEnabled) return NotFound();
        HistoryEnabled = config.HistoryEnabled;
        if (!ModelState.IsValid) return Page();

        var p = await _db.CustomerProfiles.FirstOrDefaultAsync(x => x.UserId == UserId);
        if (p is null)
        {
            p = new CustomerProfile { UserId = UserId };
            _db.CustomerProfiles.Add(p);
        }
        p.FirstName = Input.FirstName.Trim();
        p.LastName = Input.LastName.Trim();
        p.Phone = Input.Phone?.Trim();
        p.AddressLine = Input.AddressLine?.Trim();
        p.PostalCode = Input.PostalCode?.Trim();
        p.City = Input.City?.Trim();
        p.BillingSameAsDelivery = Input.BillingSameAsDelivery;
        p.BillingName = Input.BillingSameAsDelivery ? null : Input.BillingName?.Trim();
        p.BillingAddressLine = Input.BillingSameAsDelivery ? null : Input.BillingAddressLine?.Trim();
        p.BillingPostalCode = Input.BillingSameAsDelivery ? null : Input.BillingPostalCode?.Trim();
        p.BillingCity = Input.BillingSameAsDelivery ? null : Input.BillingCity?.Trim();
        p.UpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["CartMessage"] = "Votre profil est enregistré.";
        return RedirectToPage();
    }
}
