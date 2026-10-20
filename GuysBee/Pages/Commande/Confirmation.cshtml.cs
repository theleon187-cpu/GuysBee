using System.Security.Claims;
using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages.Commande;

public class ConfirmationModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ShopSettings _shop;

    public ConfirmationModel(AppDbContext db, ShopSettings shop)
    {
        _db = db;
        _shop = shop;
    }

    public Order Order { get; private set; } = null!;
    public bool HistoryEnabled { get; private set; }

    public async Task<IActionResult> OnGetAsync(string number)
    {
        var config = await _shop.GetAsync();
        if (!config.Enabled) return NotFound();
        var order = await _db.Orders.AsNoTracking().Include(o => o.Lines).FirstOrDefaultAsync(o => o.Number == number);
        if (order is null) return NotFound();

        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var justOrdered = TempData.Peek("OrderNumber") as string == number;
        if (!justOrdered && (uid is null || order.UserId != uid)) return NotFound();

        Order = order;
        HistoryEnabled = config.HistoryEnabled;
        return Page();
    }
}
