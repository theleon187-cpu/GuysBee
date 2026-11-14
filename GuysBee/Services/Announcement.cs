using System.Globalization;

namespace GuysBee.Services;

public static class AnnouncementKeys
{
    public const string Enabled = "annonce.enabled";
    public const string Title = "annonce.title";
    public const string Message = "annonce.message";
    public const string LinkUrl = "annonce.linkUrl";
    public const string LinkLabel = "annonce.linkLabel";
    public const string Style = "annonce.style";
    public const string Display = "annonce.display";
    public const string Scope = "annonce.scope";
    public const string Start = "annonce.start";
    public const string End = "annonce.end";
    public const string Closable = "annonce.closable";
    public const string CookieDays = "annonce.cookieDays";
    public const string Delay = "annonce.delay";
    public const string Version = "annonce.version";

    public const string CookieName = "gb_annonce";

    public static readonly string[] All =
    [
        Enabled, Title, Message, LinkUrl, LinkLabel, Style, Display, Scope,
        Start, End, Closable, CookieDays, Delay, Version
    ];
}

/// <summary>Annonce prête à être affichée sur le site public.</summary>
public record AnnouncementView(
    string Title, string Message, string LinkUrl, string LinkLabel,
    string Style, string Display, bool Closable, int CookieDays, int DelaySeconds, string Version);

public static class AnnouncementResolver
{
    public static readonly string[] Styles = ["info", "promo", "alerte"];
    public static readonly string[] Displays = ["bandeau", "fenetre"];

    /// <summary>Retourne l'annonce à afficher, ou null si elle est désactivée, hors période, ou hors page ciblée.</summary>
    public static AnnouncementView? Resolve(IReadOnlyDictionary<string, string> s, string path, DateTime now)
    {
        string V(string k) => s.TryGetValue(k, out var v) ? v ?? "" : "";
        if (V(AnnouncementKeys.Enabled) != "1") return null;
        var message = V(AnnouncementKeys.Message);
        if (string.IsNullOrWhiteSpace(message)) return null;

        if (V(AnnouncementKeys.Scope) == "accueil" && !(path == "/" || path.Equals("/Index", StringComparison.OrdinalIgnoreCase)))
            return null;

        if (TryDate(V(AnnouncementKeys.Start), out var start) && now < start) return null;
        if (TryDate(V(AnnouncementKeys.End), out var end) && now > end.Date.AddDays(1).AddTicks(-1)) return null;

        var style = Array.IndexOf(Styles, V(AnnouncementKeys.Style)) >= 0 ? V(AnnouncementKeys.Style) : "info";
        var display = Array.IndexOf(Displays, V(AnnouncementKeys.Display)) >= 0 ? V(AnnouncementKeys.Display) : "bandeau";
        var link = V(AnnouncementKeys.LinkUrl);
        var label = V(AnnouncementKeys.LinkLabel);
        if (string.IsNullOrWhiteSpace(label)) label = "En savoir plus";
        var days = int.TryParse(V(AnnouncementKeys.CookieDays), out var d) ? Math.Clamp(d, 0, 365) : 7;
        var delay = int.TryParse(V(AnnouncementKeys.Delay), out var dl) ? Math.Clamp(dl, 0, 60) : 0;
        var closable = V(AnnouncementKeys.Closable) != "0";
        var version = string.IsNullOrWhiteSpace(V(AnnouncementKeys.Version)) ? "1" : V(AnnouncementKeys.Version);

        return new AnnouncementView(V(AnnouncementKeys.Title), message, link, label, style, display, closable, days, delay, version);
    }

    public static bool TryDate(string value, out DateTime date) =>
        DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}
