using System.Security.Claims;
using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages.Compte.Commandes;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ShopSettings _shop;

    public IndexModel(AppDbContext db, ShopSettings shop)
    {
        _db = db;
        _shop = shop;
    }

    public List<Order> Orders { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (!(await _shop.GetAsync()).HistoryEnabled) return NotFound();
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Orders = await _db.Orders.AsNoTracking().Where(o => o.UserId == uid)
            .OrderByDescending(o => o.CreatedUtc).ToListAsync();
        return Page();
    }

    public static string StatusLabel(OrderStatus s) => s switch
    {
        OrderStatus.Pending => "En attente de règlement",
        OrderStatus.Paid => "Payée",
        OrderStatus.Preparing => "En préparation",
        OrderStatus.Shipped => "Expédiée",
        OrderStatus.Delivered => "Livrée",
        OrderStatus.Cancelled => "Annulée",
        OrderStatus.Refunded => "Remboursée",
        _ => s.ToString(),
    };
}
