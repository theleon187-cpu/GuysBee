namespace GuysBee.Services;

public static class FontChoices
{
    public const string TitleKey = "font.title";
    public const string TextKey = "font.text";
    public const string BrandKey = "font.brand";

    public static readonly (string Id, string Label, string Stack)[] All =
    [
        ("", "Police par défaut du site", ""),
        ("georgia", "Georgia (classique, avec empattements)", "Georgia, \"Times New Roman\", serif"),
        ("palatino", "Palatino (élégante, artisanale)", "\"Iowan Old Style\", \"Palatino Linotype\", Palatino, Georgia, serif"),
        ("times", "Times (journal)", "\"Times New Roman\", Times, serif"),
        ("system", "Moderne (sans empattement)", "system-ui, -apple-system, \"Segoe UI\", Roboto, sans-serif"),
        ("verdana", "Verdana (large, très lisible)", "Verdana, Geneva, sans-serif"),
        ("trebuchet", "Trebuchet (chaleureuse)", "\"Trebuchet MS\", Tahoma, sans-serif"),
        ("tahoma", "Tahoma (compacte)", "Tahoma, Geneva, sans-serif"),
        ("courier", "Machine à écrire", "\"Courier New\", Courier, monospace"),
        ("cursive", "Écriture manuscrite", "\"Segoe Script\", \"Comic Sans MS\", cursive"),
    ];

    public static string? StackFor(string? id) =>
        string.IsNullOrEmpty(id) ? null : All.FirstOrDefault(f => f.Id == id).Stack is { Length: > 0 } s ? s : null;

    public static bool IsValid(string? id) => string.IsNullOrEmpty(id) || All.Any(f => f.Id == id);
}
