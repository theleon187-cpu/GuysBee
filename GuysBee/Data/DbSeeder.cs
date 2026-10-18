using GuysBee.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Data;

/// <summary>
/// Données initiales de GUY's Bee. À appeler après application des migrations.
/// Idempotent : ne fait rien si les données existent déjà.
/// </summary>
public static class DbSeeder
{
    // Poids d'expédition = miel + pot + emballage. Estimations à vérifier en pesant un colis réel.
    private const int Shipping500g = 800;
    private const int Shipping1kg = 1400;

    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.SiteSettings.AnyAsync())
        {
            db.SiteSettings.AddRange(
                new SiteSetting { Key = "business.name", Value = "GUY's Bee" },
                new SiteSetting { Key = "business.postalCode", Value = "69690" },
                new SiteSetting { Key = "business.city", Value = "Saint-Julien-sur-Bibost" },
                new SiteSetting { Key = "business.phone", Value = "06 50 51 12 36" },
                new SiteSetting { Key = "business.email", Value = "GUYsBee@hotmail.com" },
                new SiteSetting { Key = "social.instagram", Value = "GUYs.Bee" },
                new SiteSetting { Key = "social.facebook", Value = "GUY’S Bee" });
        }

        if (!await db.Categories.AnyAsync())
        {
            var miels = new Category { Name = "Nos miels", Slug = "miels", SortOrder = 1 };
            db.Categories.Add(miels);

            (string Name, string Slug, string Short, decimal P500, decimal P1000)[] honeys =
            [
                ("Miel de printemps", "miel-de-printemps", "Le premier miel de la saison, récolté après les floraisons du printemps.", 9.10m, 16.80m),
                ("Miel d’acacia", "miel-d-acacia", "Un miel clair et doux, issu des fleurs d’acacia.", 11.20m, 21.00m),
                ("Miel de montagne", "miel-de-montagne", "Un miel récolté en altitude, au goût plus affirmé.", 10.50m, 19.60m),
                ("Miel de fleurs sauvages", "miel-de-fleurs-sauvages", "Un miel de miellées variées, à l’image de nos prairies.", 9.80m, 18.20m),
            ];

            var order = 1;
            foreach (var h in honeys)
            {
                db.Products.Add(new Product
                {
                    Name = h.Name,
                    Slug = h.Slug,
                    ShortDescription = h.Short,
                    Category = miels,
                    IsSeasonal = true,
                    SortOrder = order++,
                    Variants =
                    {
                        new ProductVariant { Label = "Pot 500 g", Price = h.P500, ShippingWeightGrams = Shipping500g, Stock = 0 },
                        new ProductVariant { Label = "Pot 1 kg", Price = h.P1000, ShippingWeightGrams = Shipping1kg, Stock = 0 },
                    }
                });
            }
        }

        if (!await db.ShippingMethods.AnyAsync())
        {
            // Seul le retrait est actif par défaut : l'apiculteur règle le reste avec l'assistant.
            db.ShippingMethods.AddRange(
                new ShippingMethod
                {
                    Name = "Retrait sur place",
                    Kind = ShippingKind.Pickup,
                    IsActive = true,
                    PreparationDays = 2,
                    CustomerMessage = "Retrait à Saint-Julien-sur-Bibost, sur rendez-vous.",
                    SortOrder = 1
                },
                new ShippingMethod
                {
                    Name = "Colis à domicile",
                    Kind = ShippingKind.Parcel,
                    IsActive = false,
                    PreparationDays = 2,
                    DeliveryMinDays = 2,
                    DeliveryMaxDays = 4,
                    FreeAboveAmount = 60m,
                    CustomerMessage = "Expédié en France métropolitaine.",
                    SortOrder = 2,
                    Zones =
                    {
                        new ShippingZone
                        {
                            Name = "France métropolitaine",
                            Rates =
                            {
                                new ShippingRate { MinGrams = 0, MaxGrams = 1000, Price = 6.90m },
                                new ShippingRate { MinGrams = 1001, MaxGrams = 3000, Price = 9.90m },
                                new ShippingRate { MinGrams = 3001, MaxGrams = 10000, Price = 14.90m },
                            }
                        }
                    }
                },
                new ShippingMethod
                {
                    Name = "Livraison locale",
                    Kind = ShippingKind.LocalRound,
                    IsActive = false,
                    PreparationDays = 2,
                    CustomerMessage = "Tournée locale, jours communiqués par l'apiculteur.",
                    SortOrder = 3,
                    Zones =
                    {
                        new ShippingZone { Name = "Rhône (69)", PostalPrefixes = "69", FlatPrice = 4.00m }
                    }
                });
        }

        await db.SaveChangesAsync();
    }
}
