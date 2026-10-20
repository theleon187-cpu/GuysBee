namespace GuysBee.Data.Entities;

public enum OrderStatus { Pending = 0, Paid = 1, Preparing = 2, Shipped = 3, Delivered = 4, Cancelled = 5, Refunded = 6 }

public class Order
{
    public int Id { get; set; }
    public string Number { get; set; } = "";
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string? CustomerPhone { get; set; }
    public string? AddressLine { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = "FR";
    public int? ShippingMethodId { get; set; }
    public ShippingMethod? ShippingMethod { get; set; }
    public string? ShippingMethodLabel { get; set; }
    public decimal ItemsTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public string? CustomerNote { get; set; }
    public string? UserId { get; set; }
    public string? BillingName { get; set; }
    public string? BillingAddressLine { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCity { get; set; }
    /// <summary>Point relais choisi par le client (nom et adresse).</summary>
    public string? RelayPointLabel { get; set; }
    public string? TrackingNumber { get; set; }
    public string? PayPalOrderId { get; set; }
    public string? PayPalCaptureId { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PaidUtc { get; set; }
    public List<OrderLine> Lines { get; set; } = new();
}

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int? ProductVariantId { get; set; }
    public string Label { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int WeightGrams { get; set; }
}

public enum ShippingKind { Pickup = 0, Parcel = 1, LocalRound = 2, Relay = 3 }

public class ShippingMethod
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ShippingKind Kind { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CustomerMessage { get; set; }
    public int PreparationDays { get; set; } = 2;
    public int? DeliveryMinDays { get; set; }
    public int? DeliveryMaxDays { get; set; }
    /// <summary>Commande offerte au-dessus de ce montant (null = jamais).</summary>
    public decimal? FreeAboveAmount { get; set; }
    public int SortOrder { get; set; }
    public List<ShippingZone> Zones { get; set; } = new();
}

public class ShippingZone
{
    public int Id { get; set; }
    public int ShippingMethodId { get; set; }
    public ShippingMethod? ShippingMethod { get; set; }
    public string Name { get; set; } = "";
    /// <summary>Départements ou préfixes de code postal, séparés par des virgules (vide = partout).</summary>
    public string? PostalPrefixes { get; set; }
    public decimal? FlatPrice { get; set; }
    public List<ShippingRate> Rates { get; set; } = new();
}

public class ShippingRate
{
    public int Id { get; set; }
    public int ShippingZoneId { get; set; }
    public ShippingZone? ShippingZone { get; set; }
    /// <summary>Tranche de poids : de MinGrams (inclus) à MaxGrams (inclus).</summary>
    public int MinGrams { get; set; }
    public int MaxGrams { get; set; }
    public decimal Price { get; set; }
}
