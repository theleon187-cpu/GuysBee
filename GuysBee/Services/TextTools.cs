using System.Globalization;
using System.Text;

namespace GuysBee.Services;

public static class TextTools
{
    /// <summary>Transforme un libellé en identifiant d'adresse (minuscules, sans accents, tirets).</summary>
    public static string Slugify(string text)
    {
        var chars = text.ToLowerInvariant().Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        var slug = new string(chars);
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }

    /// <summary>Adresse interne au site (commence par un seul « / »), ou adresse web http(s).</summary>
    public static bool IsSafeUrl(string? url, bool allowExternal = true)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        url = url.Trim();
        if (url.StartsWith('/'))
            return url.Length == 1 || (url[1] != '/' && url[1] != '\\');
        return allowExternal
            && Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }
}
