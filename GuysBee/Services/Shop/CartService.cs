using System.Text.Json;
using GuysBee.Data;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Services.Shop;

public record CartItem(int VariantId, string Label, decimal UnitPrice, int Quantity, int WeightGrams)
{
    public decimal LineTotal => UnitPrice * Quantity;
}

/// <summary>Panier conservé en session (identifiant de conditionnement, quantité).</summary>
public class CartService
{
    private const string Key = "cart";
    private readonly IHttpContextAccessor _accessor;
    private readonly AppDbContext _db;

    public CartService(IHttpContextAccessor accessor, AppDbContext db)
    {
        _accessor = accessor;
        _db = db;
    }

    private ISession? Session => _accessor.HttpContext?.Session;

    private Dictionary<int, int> Read()
    {
        var json = Session?.GetString(Key);
        return string.IsNullOrEmpty(json)
            ? new Dictionary<int, int>()
            : JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? new Dictionary<int, int>();
    }

    private void Write(Dictionary<int, int> cart) => Session?.SetString(Key, JsonSerializer.Serialize(cart));

    public int Count => Read().Values.Sum();

    public void Add(int variantId, int quantity)
    {
        var cart = Read();
        cart[variantId] = Math.Clamp(cart.GetValueOrDefault(variantId) + quantity, 1, 99);
        Write(cart);
    }

    public void SetQuantity(int variantId, int quantity)
    {
        var cart = Read();
        if (quantity <= 0) cart.Remove(variantId);
        else cart[variantId] = Math.Min(quantity, 99);
        Write(cart);
    }

    public void Clear() => Session?.Remove(Key);

    /// <summary>Lignes valides : conditionnements actifs, quantité plafonnée au stock.</summary>
    public async Task<List<CartItem>> LoadAsync()
    {
        var cart = Read();
        if (cart.Count == 0) return new List<CartItem>();
        var ids = cart.Keys.ToList();
        var variants = await _db.ProductVariants.AsNoTracking().Include(v => v.Product)
            .Where(v => ids.Contains(v.Id) && v.IsActive && v.Product!.IsActive && v.Stock > 0)
            .ToListAsync();
        return variants
            .Select(v => new CartItem(v.Id, $"{v.Product!.Name} - {v.Label}", v.Price, Math.Min(cart[v.Id], v.Stock), v.ShippingWeightGrams))
            .OrderBy(i => i.Label)
            .ToList();
    }
}
