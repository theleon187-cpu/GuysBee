using System.Security.Claims;
using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Pages.Compte.Commandes;

public class DetailModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ShopSettings _shop;

    public DetailModel(AppDbContext db, ShopSettings shop)
    {
        _db = db;
        _shop = shop;
    }

    public Order Order { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(string number)
    {
        if (!(await _shop.GetAsync()).HistoryEnabled) return NotFound();
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = await _db.Orders.AsNoTracking().Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Number == number && o.UserId == uid);
        if (order is null) return NotFound();
        Order = order;
        return Page();
    }
}
