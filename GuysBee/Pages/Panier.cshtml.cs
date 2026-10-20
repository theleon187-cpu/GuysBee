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
        TempData["CartMessage"] = "Article ajouté au panier.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int variantId, int quantity)
    {
        if (!(await _shop.GetAsync()).Enabled) return NotFound();
        _cart.SetQuantity(variantId, quantity);
        return RedirectToPage();
    }
}
