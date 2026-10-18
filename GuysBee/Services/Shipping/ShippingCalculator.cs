using GuysBee.Data.Entities;

namespace GuysBee.Services.Shipping;

public record CartLine(int WeightGrams, int Quantity, decimal UnitPrice);

public record ShippingQuote(int MethodId, string MethodName, decimal Price, bool IsFree, string? Message, int? MinDays, int? MaxDays);

/// <summary>Calcul des frais de livraison, sans accès base : testable facilement.</summary>
public static class ShippingCalculator
{
    public static IReadOnlyList<ShippingQuote> Quote(
        IEnumerable<ShippingMethod> methods,
        IReadOnlyCollection<CartLine> cart,
        string? postalCode)
    {
        var itemsTotal = cart.Sum(l => l.UnitPrice * l.Quantity);
        var weight = cart.Sum(l => l.WeightGrams * l.Quantity);
        var result = new List<ShippingQuote>();

        foreach (var m in methods.Where(m => m.IsActive).OrderBy(m => m.SortOrder))
        {
            var price = ComputePrice(m, weight, postalCode);
            if (price is null) continue;

            var free = m.FreeAboveAmount is { } threshold && itemsTotal >= threshold;
            result.Add(new ShippingQuote(
                m.Id, m.Name, free ? 0m : price.Value, free, m.CustomerMessage,
                m.DeliveryMinDays is null ? null : m.DeliveryMinDays + m.PreparationDays,
                m.DeliveryMaxDays is null ? null : m.DeliveryMaxDays + m.PreparationDays));
        }
        return result;
    }

    private static decimal? ComputePrice(ShippingMethod m, int weightGrams, string? postalCode)
    {
        if (m.Kind == ShippingKind.Pickup && m.Zones.Count == 0) return 0m;

        foreach (var zone in m.Zones)
        {
            if (!ZoneMatches(zone, postalCode)) continue;
            if (zone.Rates.Count > 0)
            {
                var rate = zone.Rates.FirstOrDefault(r => weightGrams >= r.MinGrams && weightGrams <= r.MaxGrams);
                if (rate is not null) return rate.Price;
                continue;
            }
            if (zone.FlatPrice is { } flat) return flat;
        }
        return null;
    }

    private static bool ZoneMatches(ShippingZone zone, string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(zone.PostalPrefixes)) return true;
        if (string.IsNullOrWhiteSpace(postalCode)) return false;
        return zone.PostalPrefixes
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(p => postalCode.StartsWith(p, StringComparison.Ordinal));
    }
}
