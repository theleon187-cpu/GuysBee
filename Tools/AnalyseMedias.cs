// Analyse technique des médias du dossier posts/.
// Usage (depuis C:\Dev\GuyBee\GuysBee) :  dotnet run Tools/AnalyseMedias.cs
// Option : dotnet run Tools/AnalyseMedias.cs -- "GuysBee/posts"
// Sortie : Content/analyse-techniques.md
#:package SixLabors.ImageSharp@3.1.*

using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

var root = args.Length > 0 ? args[0] : Path.Combine("GuysBee", "posts");
if (!Directory.Exists(root))
{
    Console.WriteLine($"Dossier introuvable : {Path.GetFullPath(root)}");
    return 1;
}

var extensions = new[] { ".webp", ".jpg", ".jpeg", ".png" };
var files = Directory.GetFiles(root)
    .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
    .ToList();

var rows = new List<Row>();
foreach (var file in files)
{
    try
    {
        using var img = Image.Load<Rgb24>(file);
        string? exifDate = null;
        if (img.Metadata.ExifProfile is { } exif
            && exif.TryGetValue(ExifTag.DateTimeOriginal, out var dt))
        {
            exifDate = dt.Value;
        }

        using var small = img.Clone(c => c.Resize(8, 8));
        var grays = new double[64];
        for (var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x++)
            {
                var p = small[x, y];
                grays[y * 8 + x] = 0.299 * p.R + 0.587 * p.G + 0.114 * p.B;
            }
        var avg = grays.Average();
        ulong hash = 0;
        for (var i = 0; i < 64; i++)
            if (grays[i] >= avg) hash |= 1UL << i;

        using var one = img.Clone(c => c.Resize(1, 1));
        var px = one[0, 0];

        rows.Add(new Row(
            Path.GetFileName(file),
            img.Width,
            img.Height,
            new FileInfo(file).Length,
            hash,
            $"#{px.R:X2}{px.G:X2}{px.B:X2}",
            avg,
            exifDate));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Illisible : {file} ({ex.Message})");
    }
}

static string Orientation(int w, int h) =>
    Math.Abs(w - h) < w * 0.05 ? "carré" : w > h ? "paysage" : "portrait";

static int Hamming(ulong a, ulong b) => System.Numerics.BitOperations.PopCount(a ^ b);

var sb = new StringBuilder();
sb.AppendLine("# Analyse technique des médias");
sb.AppendLine();
sb.AppendLine($"Dossier : `{root}` : {rows.Count} image(s).");
sb.AppendLine();
sb.AppendLine("| Fichier | Taille px | Orientation | Poids (Ko) | Couleur moyenne | Luminosité | Date EXIF | Alerte |");
sb.AppendLine("|---|---|---|---|---|---|---|---|");
foreach (var r in rows)
{
    var alerts = new List<string>();
    if (Math.Min(r.W, r.H) < 800) alerts.Add("petite résolution");
    if (r.Brightness < 60) alerts.Add("sombre");
    if (r.Brightness > 200) alerts.Add("très claire");
    sb.AppendLine($"| {r.Name} | {r.W}x{r.H} | {Orientation(r.W, r.H)} | {r.Bytes / 1024} | {r.Color} | {r.Brightness:F0} | {r.Exif ?? "-"} | {string.Join(", ", alerts)} |");
}

sb.AppendLine();
sb.AppendLine("## Doublons ou images très proches");
sb.AppendLine();
var found = false;
for (var i = 0; i < rows.Count; i++)
    for (var j = i + 1; j < rows.Count; j++)
    {
        var d = Hamming(rows[i].Hash, rows[j].Hash);
        if (d <= 6)
        {
            found = true;
            sb.AppendLine($"- `{rows[i].Name}` et `{rows[j].Name}` (distance {d}/64)");
        }
    }
if (!found) sb.AppendLine("Aucun doublon probable détecté.");

sb.AppendLine();
sb.AppendLine("## Synthèse");
sb.AppendLine();
sb.AppendLine($"- Paysage : {rows.Count(r => Orientation(r.W, r.H) == "paysage")}");
sb.AppendLine($"- Portrait : {rows.Count(r => Orientation(r.W, r.H) == "portrait")}");
sb.AppendLine($"- Carré : {rows.Count(r => Orientation(r.W, r.H) == "carré")}");
sb.AppendLine($"- Poids total : {rows.Sum(r => r.Bytes) / 1024 / 1024.0:F1} Mo");

Directory.CreateDirectory("Content");
File.WriteAllText(Path.Combine("Content", "analyse-techniques.md"), sb.ToString(), Encoding.UTF8);
Console.WriteLine("Rapport écrit : Content/analyse-techniques.md");
return 0;

record Row(string Name, int W, int H, long Bytes, ulong Hash, string Color, double Brightness, string? Exif);
