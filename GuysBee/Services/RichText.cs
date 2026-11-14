using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;

namespace GuysBee.Services;

/// <summary>Affiche un texte saisi soit en HTML (éditeur visuel, déjà assaini), soit en texte simple (anciennes données).</summary>
public static class RichText
{
    private static readonly Regex TagLike = new(@"<\s*/?\s*(p|br|h2|h3|strong|b|em|i|u|ul|ol|li|a|blockquote)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IHtmlContent Render(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return HtmlString.Empty;
        if (TagLike.IsMatch(value)) return new HtmlString(value);
        return new HtmlString(WebUtility.HtmlEncode(value.Trim()).Replace("\r\n", "\n").Replace("\n", "<br />"));
    }
}
