namespace GuysBee.Data.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Description { get; set; }
    public string? MenuLabel { get; set; }
    public int SortOrder { get; set; }
    public List<Product> Products { get; set; } = new();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string ShortDescription { get; set; } = "";
    public string? LongDescription { get; set; }
    /// <summary>Origine florale ou récolte (ex. printemps, châtaignier).</summary>
    public string? Origin { get; set; }
    public string? Ingredients { get; set; }
    public string? LabelMentions { get; set; }
    public string? UsageTips { get; set; }
    public bool IsSeasonal { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public int? MainMediaId { get; set; }
    public MediaAsset? MainMedia { get; set; }
    public List<ProductVariant> Variants { get; set; } = new();
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    /// <summary>Ex. « Pot 250 g ».</summary>
    public string Label { get; set; } = "";
    public decimal Price { get; set; }
    /// <summary>Poids d'expédition en grammes, emballage compris.</summary>
    public int ShippingWeightGrams { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum MediaKind { Image = 0, Video = 1 }
public enum MediaStatus { Draft = 0, Published = 1, Hidden = 2 }

public class MediaAsset
{
    public int Id { get; set; }
    public string SourceFileName { get; set; } = "";
    public string BaseName { get; set; } = "";
    /// <summary>Empreinte SHA-256 du fichier source (détection des doublons).</summary>
    public string ContentHash { get; set; } = "";
    public MediaKind Kind { get; set; }
    public MediaStatus Status { get; set; } = MediaStatus.Draft;
    public int Width { get; set; }
    public int Height { get; set; }
    public string? Caption { get; set; }
    public string? AltText { get; set; }
    /// <summary>Thème : ruche, récolte, produit, paysage, etc.</summary>
    public string? Theme { get; set; }
    public string? Season { get; set; }
    public DateTime? TakenOn { get; set; }
    public int SortOrder { get; set; }
    /// <summary>Afficher cette photo dans le bandeau d'accueil (gb-hero).</summary>
    public bool ShowInHero { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Body { get; set; } = "";
    public int? CoverMediaId { get; set; }
    public MediaAsset? CoverMedia { get; set; }
    public bool IsPublished { get; set; }
    public DateTime PublishedUtc { get; set; } = DateTime.UtcNow;
}

public class SalePoint
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string City { get; set; } = "";
    public string? Schedule { get; set; }
    public string? Phone { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SiteSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}
