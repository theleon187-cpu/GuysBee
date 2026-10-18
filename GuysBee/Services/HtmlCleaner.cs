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
                     "a", "img", "blockquote", "table", "thead", "tbody", "tr", "th", "td", "hr"
                 })
        {
            _sanitizer.AllowedTags.Add(tag);
        }

        _sanitizer.AllowedAttributes.Clear();
        foreach (var attr in new[] { "href", "target", "rel", "src", "alt", "title", "width", "height", "colspan", "rowspan" })
        {
            _sanitizer.AllowedAttributes.Add(attr);
        }

        _sanitizer.AllowedSchemes.Clear();
        foreach (var scheme in new[] { "http", "https", "mailto", "tel" })
        {
            _sanitizer.AllowedSchemes.Add(scheme);
        }
        // Pas de style inline autorisé (règle du projet).
        _sanitizer.AllowedCssProperties.Clear();
    }

    public string Clean(string? html) => string.IsNullOrWhiteSpace(html) ? "" : _sanitizer.Sanitize(html);
}
