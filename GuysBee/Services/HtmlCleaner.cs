using Ganss.Xss;

namespace GuysBee.Services;

/// <summary>Assainit le HTML saisi dans l'éditeur visuel (anti-XSS) avant enregistrement.</summary>
public interface IHtmlCleaner
{
    string Clean(string? html);
}

public class HtmlCleaner : IHtmlCleaner
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlCleaner()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
                 {
                     "p", "br", "h2", "h3", "strong", "b", "em", "i", "u", "ul", "ol", "li",
                     "a", "img", "blockquote", "table", "thead", "tbody", "tr", "th", "td", "hr", "span", "s", "strike"
                 })
        {
            _sanitizer.AllowedTags.Add(tag);
        }

        _sanitizer.AllowedAttributes.Clear();
        foreach (var attr in new[] { "href", "target", "rel", "src", "alt", "title", "width", "height", "colspan", "rowspan", "style" })
        {
            _sanitizer.AllowedAttributes.Add(attr);
        }

        _sanitizer.AllowedSchemes.Clear();
        foreach (var scheme in new[] { "http", "https", "mailto", "tel" })
        {
            _sanitizer.AllowedSchemes.Add(scheme);
        }
        // Seules la mise en forme du texte (couleur, taille, alignement) est tolérée en style inline, produite par l'éditeur visuel.
        _sanitizer.AllowedCssProperties.Clear();
        foreach (var prop in new[] { "color", "background-color", "font-size", "text-align", "text-decoration", "margin-left", "padding-left" })
        {
            _sanitizer.AllowedCssProperties.Add(prop);
        }
    }

    public string Clean(string? html) => string.IsNullOrWhiteSpace(html) ? "" : _sanitizer.Sanitize(html);
}
