using System.Security.Cryptography;
using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace GuysBee.Services;

public static class MediaUrls
{
    public static readonly int[] Widths = [480, 800, 1200];

    public static string Src(MediaAsset m, int width = 800) => $"/media/{m.BaseName}-{width}.webp";

    public static string SrcSet(MediaAsset m) =>
        string.Join(", ", Widths.Select(w => $"{Src(m, w)} {w}w"));

    public static string Alt(MediaAsset m) =>
        !string.IsNullOrWhiteSpace(m.AltText) ? m.AltText!
        : !string.IsNullOrWhiteSpace(m.Caption) ? m.Caption!
        : "Photo de GUY's Bee";
}

public record ImportResult(int Imported, int Duplicates, int Errors);

/// <summary>Import et optimisation des images : WebP en 480/800/1200 px, dédoublonnage par empreinte.</summary>
public class MediaService
{
    private static readonly string[] Extensions = [".webp", ".jpg", ".jpeg", ".png"];

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger<MediaService> _logger;

    public MediaService(AppDbContext db, IWebHostEnvironment env, IConfiguration config, ILogger<MediaService> logger)
    {
        _db = db;
        _env = env;
        _config = config;
        _logger = logger;
    }

    private string OutputDir => Path.Combine(_env.WebRootPath, "media");

    public string SourceFolder =>
        _config["Media:SourceFolder"] is { Length: > 0 } custom
            ? custom
            : Path.Combine(_env.ContentRootPath, "posts");

    public async Task<ImportResult> ImportFolderAsync(CancellationToken ct = default)
    {
        if (!Directory.Exists(SourceFolder)) return new ImportResult(0, 0, 0);

        int imported = 0, dups = 0, errors = 0;
        var files = Directory.GetFiles(SourceFolder)
            .Where(f => Extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            try
            {
                var bytes = await File.ReadAllBytesAsync(file, ct);
                var asset = await AddAsync(bytes, Path.GetFileName(file), ct);
                if (asset is null) dups++; else imported++;
            }
            catch (Exception ex)
            {
                errors++;
                _logger.LogWarning(ex, "Import impossible pour {File}", file);
            }
        }
        return new ImportResult(imported, dups, errors);
    }

    public async Task<MediaAsset?> AddUploadAsync(IFormFile file, CancellationToken ct = default)
    {
        if (!Extensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
            throw new InvalidOperationException("Format non accepté (WebP, JPG ou PNG).");
        if (file.Length > 15 * 1024 * 1024)
            throw new InvalidOperationException("Fichier trop volumineux (15 Mo maximum).");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        return await AddAsync(ms.ToArray(), Path.GetFileName(file.FileName), ct);
    }

    /// <summary>Retourne null si l'image existe déjà (même contenu).</summary>
    private async Task<MediaAsset?> AddAsync(byte[] bytes, string sourceName, CancellationToken ct)
    {
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (await _db.MediaAssets.AnyAsync(m => m.ContentHash == hash, ct)) return null;

        using var image = await Image.LoadAsync(new MemoryStream(bytes), ct);
        var width = image.Width;
        var height = image.Height;

        var baseName = "m-" + hash[..12];
        Directory.CreateDirectory(OutputDir);
        var encoder = new WebpEncoder { Quality = 80 };

        foreach (var w in MediaUrls.Widths)
        {
            var target = Math.Min(w, width);
            using var clone = image.Clone(c => c.Resize(target, 0));
            await clone.SaveAsync(Path.Combine(OutputDir, $"{baseName}-{w}.webp"), encoder, ct);
        }

        if (await _db.MediaAssets.AnyAsync(m => m.SourceFileName == sourceName, ct))
            sourceName = $"{hash[..8]}-{sourceName}";

        var asset = new MediaAsset
        {
            SourceFileName = sourceName,
            BaseName = baseName,
            ContentHash = hash,
            Kind = MediaKind.Image,
            Status = MediaStatus.Draft,
            Width = width,
            Height = height,
            SortOrder = await _db.MediaAssets.CountAsync(ct) + 1
        };
        _db.MediaAssets.Add(asset);
        await _db.SaveChangesAsync(ct);
        return asset;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var asset = await _db.MediaAssets.FindAsync([id], ct);
        if (asset is null) return;

        foreach (var w in MediaUrls.Widths)
        {
            var path = Path.Combine(OutputDir, $"{asset.BaseName}-{w}.webp");
            if (File.Exists(path)) File.Delete(path);
        }
        _db.MediaAssets.Remove(asset);
        await _db.SaveChangesAsync(ct);
    }
}
