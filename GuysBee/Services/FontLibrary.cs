using System.Text;
using System.Text.RegularExpressions;

namespace GuysBee.Services;

public class FontLibrary
{
    public const long MaxBytes = 2 * 1024 * 1024;
    private static readonly string[] Extensions = [".woff2", ".woff", ".ttf", ".otf"];
    private readonly string _dir;

    public FontLibrary(IWebHostEnvironment env)
    {
        _dir = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "fonts", "custom");
    }

    public static string FamilyOf(string id) => "GBCustom-" + Path.GetFileNameWithoutExtension(id.Substring("custom:".Length));

    public List<(string Id, string Label, string Stack)> Custom()
    {
        if (!Directory.Exists(_dir)) return new();
        return Directory.EnumerateFiles(_dir)
            .Where(f => Extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .Select(f => Path.GetFileName(f))
            .OrderBy(f => f)
            .Select(f => ("custom:" + f, "Importée : " + Path.GetFileNameWithoutExtension(f), $"\"{FamilyOf("custom:" + f)}\", sans-serif"))
            .ToList();
    }

    public List<(string Id, string Label, string Stack)> All() => FontChoices.All.Concat(Custom()).ToList();

    public bool IsValid(string? id) => string.IsNullOrEmpty(id) || All().Any(f => f.Id == id);

    public string? StackFor(string? id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        var stack = All().FirstOrDefault(f => f.Id == id).Stack;
        return string.IsNullOrEmpty(stack) ? null : stack;
    }

    public string FontFaceCss()
    {
        var sb = new StringBuilder();
        foreach (var (id, _, _) in Custom())
        {
            var file = id.Substring("custom:".Length);
            var format = Path.GetExtension(file).ToLowerInvariant() switch
            {
                ".woff2" => "woff2",
                ".woff" => "woff",
                ".otf" => "opentype",
                _ => "truetype"
            };
            sb.Append($"@font-face {{ font-family: \"{FamilyOf(id)}\"; src: url(\"/fonts/custom/{Uri.EscapeDataString(file)}\") format(\"{format}\"); font-display: swap; }}\n");
        }
        return sb.ToString();
    }

    public async Task<string?> SaveAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Extensions.Contains(ext)) return "Format non pris en charge (WOFF2, WOFF, TTF ou OTF).";
        if (file.Length == 0 || file.Length > MaxBytes) return "Le fichier est vide ou dépasse 2 Mo.";
        var name = Regex.Replace(Path.GetFileNameWithoutExtension(file.FileName).ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        if (name.Length == 0) name = "police";
        if (name.Length > 40) name = name[..40];
        Directory.CreateDirectory(_dir);
        await using var fs = File.Create(Path.Combine(_dir, name + ext));
        await file.CopyToAsync(fs);
        return null;
    }

    public void Delete(string id)
    {
        if (!id.StartsWith("custom:")) return;
        var file = Path.GetFileName(id.Substring("custom:".Length));
        var path = Path.Combine(_dir, file);
        if (File.Exists(path)) File.Delete(path);
    }
}
