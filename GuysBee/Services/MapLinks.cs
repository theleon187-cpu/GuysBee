using System.Globalization;
using System.Text.RegularExpressions;

namespace GuysBee.Services;

public static partial class MapLinks
{
    public static string? Clean(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)) return null;
        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) return null;
        var host = uri.Host.ToLowerInvariant();
        var ok = host.Contains("google.") || host == "goo.gl" || host.EndsWith(".goo.gl");
        return ok ? uri.ToString() : null;
    }

    [GeneratedRegex(@"!3d(-?\d+\.\d+)!4d(-?\d+\.\d+)|@(-?\d+\.\d+),(-?\d+\.\d+)")]
    private static partial Regex CoordinatesRegex();

    /// <summary>URL d'intégration de carte : coordonnées du lien Google Maps si présentes, sinon l'adresse fournie.</summary>
    public static string? EmbedUrl(string? mapUrl, string? addressFallback)
    {
        if (string.IsNullOrWhiteSpace(mapUrl)) return null;
        string? query = null;
        var m = CoordinatesRegex().Match(Uri.UnescapeDataString(mapUrl));
        if (m.Success)
        {
            var lat = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[3].Value;
            var lng = m.Groups[2].Success ? m.Groups[2].Value : m.Groups[4].Value;
            query = string.Create(CultureInfo.InvariantCulture, $"{lat},{lng}");
        }
        else if (!string.IsNullOrWhiteSpace(addressFallback))
        {
            query = addressFallback.Trim();
        }
        return query is null ? null : "https://www.google.com/maps?hl=fr&output=embed&q=" + Uri.EscapeDataString(query);
    }
}
