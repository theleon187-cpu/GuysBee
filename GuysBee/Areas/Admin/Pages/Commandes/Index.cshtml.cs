using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Commandes;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Order> Orders { get; private set; } = new();

    [BindProperty(SupportsGet = true)] public OrderStatus? Status { get; set; }

    public async Task OnGetAsync()
    {
        var query = _db.Orders.AsNoTracking().Include(o => o.Lines).AsQueryable();
        if (Status is { } s) query = query.Where(o => o.Status == s);
        Orders = await query.OrderByDescending(o => o.CreatedUtc).Take(200).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync(int id, OrderStatus status, string? trackingNumber)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();
        order.Status = status;
        order.TrackingNumber = string.IsNullOrWhiteSpace(trackingNumber) ? null : trackingNumber.Trim();
        if (status == OrderStatus.Paid && order.PaidUtc is null) order.PaidUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Message"] = $"Commande {order.Number} mise \u00e0 jour.";
        return RedirectToPage(new { Status });
    }

    public static string StatusLabel(OrderStatus s) => global::GuysBee.Pages.Compte.Commandes.IndexModel.StatusLabel(s);
}
