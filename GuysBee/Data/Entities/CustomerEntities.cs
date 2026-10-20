namespace GuysBee.Data.Entities;

/// <summary>Profil client : coordonnées de livraison et de facturation rattachées à un compte.</summary>
public class CustomerProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Phone { get; set; }
    public string? AddressLine { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = "FR";
    public bool BillingSameAsDelivery { get; set; } = true;
    public string? BillingName { get; set; }
    public string? BillingAddressLine { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCity { get; set; }
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
