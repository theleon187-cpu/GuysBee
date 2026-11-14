using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuysBee.Pages;

public class PanierModel : PageModel
{
    private readonly CartService _cart;
    private readonly ShopSettings _shop;

    public PanierModel(CartService cart, ShopSettings shop)
    {
        _cart = cart;
        _shop = shop;
    }

    public List<CartItem> Items { get; private set; } = new();
    public decimal Total => Items.Sum(i => i.LineTotal);

    public async Task<IActionResult> OnGetAsync()
    {
        if (!(await _shop.GetAsync()).Enabled) return NotFound();
        Items = await _cart.LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(int variantId, int quantity = 1)
    {
        if (!(await _shop.GetAsync()).Enabled) return NotFound();
        _cart.Add(variantId, Math.Max(quantity, 1));
        if (Request.Headers["X-Requested-With"] == "fetch")
        {
            return new JsonResult(new { count = _cart.Count, message = "Article ajouté au panier." });
        }
        TempData["CartMessage"] = "Article ajouté au panier.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int variantId, int quantity)
    {
        if (!(await _shop.GetAsync()).Enabled) return NotFound();
        _cart.SetQuantity(variantId, quantity);
        return await CartResultAsync(variantId);
    }

    public async Task<IActionResult> OnPostRemoveAsync(int variantId)
    {
        if (!(await _shop.GetAsync()).Enabled) return NotFound();
        _cart.SetQuantity(variantId, 0);
        return await CartResultAsync(variantId);
    }

    private async Task<IActionResult> CartResultAsync(int variantId)
    {
        if (Request.Headers["X-Requested-With"] != "fetch") return RedirectToPage();
        Items = await _cart.LoadAsync();
        var line = Items.FirstOrDefault(i => i.VariantId == variantId);
        return new JsonResult(new
        {
            count = _cart.Count,
            empty = Items.Count == 0,
            quantity = line?.Quantity ?? 0,
            lineTotal = (line?.LineTotal ?? 0).ToString("0.00") + " €",
            total = Total.ToString("0.00") + " €"
        });
    }
}
