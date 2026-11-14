using System.IO.Compression;
using System.Text.Json;
using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GuysBee.Services;

/// <summary>
/// Sauvegarde complète du site : données (JSON) + photos + polices dans une archive .zip,
/// et restauration à partir d'une telle archive.
/// </summary>
public class SiteBackupService
{
    public const string LastExportKey = "backup.lastExportUtc";
    private const string DataEntry = "donnees.json";
    private const int FormatVersion = 1;
    private static readonly string[] FileFolders = { "media", Path.Combine("fonts", "custom") };

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public SiteBackupService(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private string WebRoot => _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

    private IEnumerable<IEntityType> BackedUpTypes() =>
        _db.Model.GetEntityTypes()
            .Where(t => !t.IsOwned() && t.ClrType.Namespace == typeof(Category).Namespace && t.ClrType != typeof(AuditEntry))
            .OrderBy(t => t.ClrType.Name);

    public async Task<Stream> ExportAsync()
    {
        await RegisterOrphanPhotosAsync();
        var temp = new FileStream(Path.Combine(Path.GetTempPath(), $"guysbee-{Guid.NewGuid():N}.zip"),
            FileMode.Create, FileAccess.ReadWrite, FileShare.None, 81920, FileOptions.DeleteOnClose);

        using (var zip = new ZipArchive(temp, ZipArchiveMode.Create, leaveOpen: true))
        {
            var dataEntry = zip.CreateEntry(DataEntry, CompressionLevel.Optimal);
            await using (var ds = dataEntry.Open())
            await using (var w = new Utf8JsonWriter(ds, new JsonWriterOptions { Indented = false }))
            {
                w.WriteStartObject();
                w.WriteNumber("version", FormatVersion);
                w.WriteString("createdUtc", DateTime.UtcNow);
                w.WriteStartObject("tables");
                foreach (var type in BackedUpTypes())
                {
                    w.WriteStartArray(type.ClrType.Name);
                    foreach (var row in await ReadRowsAsync(type))
                    {
                        w.WriteStartObject();
                        foreach (var p in type.GetProperties().Where(p => p.PropertyInfo is not null))
                        {
                            w.WritePropertyName(p.Name);
                            JsonSerializer.Serialize(w, p.PropertyInfo!.GetValue(row), p.ClrType);
                        }
                        w.WriteEndObject();
                    }
                    w.WriteEndArray();
                }
                w.WriteEndObject();
                w.WriteEndObject();
            }

            foreach (var folder in FileFolders)
            {
                var dir = Path.Combine(WebRoot, folder);
                if (!Directory.Exists(dir)) continue;
                foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(file).StartsWith('.')) continue;
                    var name = "fichiers/" + Path.GetRelativePath(WebRoot, file).Replace('\\', '/');
                    zip.CreateEntryFromFile(file, name, CompressionLevel.NoCompression);
                }
            }
        }

        await SaveLastExportAsync();
        temp.Position = 0;
        return temp;
    }

    private async Task<List<object>> ReadRowsAsync(IEntityType type)
    {
        var method = typeof(SiteBackupService).GetMethod(nameof(QueryAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .MakeGenericMethod(type.ClrType);
        return await (Task<List<object>>)method.Invoke(this, null)!;
    }

    private async Task<List<object>> QueryAsync<T>() where T : class =>
        (await _db.Set<T>().AsNoTracking().ToListAsync()).Cast<object>().ToList();

    /// <summary>
    /// Les photos des marchés, des lieux de revente et du logo peuvent exister sans fiche dans le gestionnaire photo :
    /// on crée la fiche manquante (en brouillon, donc non publiée dans la galerie).
    /// </summary>
    public async Task RegisterOrphanPhotosAsync()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        names.UnionWith(await _db.SalePoints.Where(s => s.PhotoBaseName != null).Select(s => s.PhotoBaseName!).ToListAsync());
        names.UnionWith(await _db.MarketEvents.Where(e => e.PhotoBaseName != null).Select(e => e.PhotoBaseName!).ToListAsync());
        names.UnionWith(await _db.SiteSettings.Where(s => s.Key == "site.logoMedia" && s.Value != "").Select(s => s.Value).ToListAsync());
        names.RemoveWhere(string.IsNullOrWhiteSpace);
        if (names.Count == 0) return;

        var known = (await _db.MediaAssets.Select(m => m.BaseName).ToListAsync()).ToHashSet(StringComparer.Ordinal);
        var order = await _db.MediaAssets.MaxAsync(m => (int?)m.SortOrder) ?? 0;
        foreach (var name in names.Where(n => !known.Contains(n)))
        {
            int width = 0, height = 0;
            var file = Path.Combine(WebRoot, "media", $"{name}-1200.webp");
            if (File.Exists(file))
            {
                try
                {
                    var info = await SixLabors.ImageSharp.Image.IdentifyAsync(file);
                    width = info.Width;
                    height = info.Height;
                }
                catch (Exception) { }
            }
            _db.MediaAssets.Add(new MediaAsset
            {
                SourceFileName = name + ".webp",
                BaseName = name,
                Kind = MediaKind.Image,
                Status = MediaStatus.Draft,
                Width = width,
                Height = height,
                SortOrder = ++order
            });
        }
        await _db.SaveChangesAsync();
    }

    private async Task SaveLastExportAsync()
    {
        var s = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Key == LastExportKey);
        var now = DateTime.UtcNow.ToString("O");
        if (s is null) _db.SiteSettings.Add(new SiteSetting { Key = LastExportKey, Value = now });
        else s.Value = now;
        await _db.SaveChangesAsync();
    }

    /// <summary>Remplace toutes les données et fichiers par ceux de l'archive. Retourne un message d'erreur ou null.</summary>
    public async Task<string?> ImportAsync(Stream archive)
    {
        ZipArchive zip;
        try { zip = new ZipArchive(archive, ZipArchiveMode.Read, leaveOpen: true); }
        catch (InvalidDataException) { return "Ce fichier n'est pas une sauvegarde valide."; }

        using (zip)
        {
            var dataEntry = zip.GetEntry(DataEntry);
            if (dataEntry is null) return "Ce fichier n'est pas une sauvegarde du site (donn\u00e9es introuvables).";

            JsonDocument doc;
            await using (var ds = dataEntry.Open())
            {
                try { doc = await JsonDocument.ParseAsync(ds); }
                catch (JsonException) { return "Les donn\u00e9es de la sauvegarde sont illisibles."; }
            }

            using (doc)
            {
                if (!doc.RootElement.TryGetProperty("version", out var v) || v.GetInt32() > FormatVersion
                    || !doc.RootElement.TryGetProperty("tables", out var tables))
                    return "Cette sauvegarde provient d'une version incompatible du site.";

                var error = await ImportDataAsync(tables);
                if (error is not null) return error;
            }

            ExtractFiles(zip);
        }
        await RegisterOrphanPhotosAsync();
        return null;
    }

    private async Task<string?> ImportDataAsync(JsonElement tables)
    {
        // Contexte d\u00e9di\u00e9, sans historisation : la restauration ne doit pas g\u00e9n\u00e9rer des milliers de lignes d'historique.
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_db.Database.GetConnectionString()).Options;
        await using var ctx = new AppDbContext(options);
        await ctx.Database.OpenConnectionAsync();
        try
        {
            await ctx.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF");
            await using var tx = await ctx.Database.BeginTransactionAsync();

            var types = ctx.Model.GetEntityTypes()
                .Where(t => !t.IsOwned() && t.ClrType.Namespace == typeof(Category).Namespace && t.ClrType != typeof(AuditEntry))
                .ToList();

            foreach (var type in types)
                await ctx.Database.ExecuteSqlRawAsync($"DELETE FROM \"{type.GetTableName()}\"");

            foreach (var type in types)
            {
                if (!tables.TryGetProperty(type.ClrType.Name, out var rows) || rows.ValueKind != JsonValueKind.Array) continue;
                var props = type.GetProperties().Where(p => p.PropertyInfo is { CanWrite: true }).ToList();
                foreach (var row in rows.EnumerateArray())
                {
                    var entity = Activator.CreateInstance(type.ClrType)!;
                    foreach (var p in props)
                    {
                        if (!row.TryGetProperty(p.Name, out var value)) continue;
                        p.PropertyInfo!.SetValue(entity, value.Deserialize(p.ClrType));
                    }
                    ctx.Add(entity);
                }
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            await tx.CommitAsync();
            return null;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or DbUpdateException or FormatException)
        {
            return "La restauration a \u00e9chou\u00e9 et aucune donn\u00e9e n'a \u00e9t\u00e9 modifi\u00e9e : " + (ex.InnerException?.Message ?? ex.Message);
        }
        finally
        {
            await ctx.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON");
            await ctx.Database.CloseConnectionAsync();
        }
    }

    private void ExtractFiles(ZipArchive zip)
    {
        var root = Path.GetFullPath(WebRoot) + Path.DirectorySeparatorChar;
        foreach (var entry in zip.Entries)
        {
            if (!entry.FullName.StartsWith("fichiers/", StringComparison.Ordinal) || entry.Name.Length == 0) continue;
            var relative = entry.FullName["fichiers/".Length..].Replace('/', Path.DirectorySeparatorChar);
            if (!FileFolders.Any(f => relative.StartsWith(f + Path.DirectorySeparatorChar, StringComparison.Ordinal))) continue;

            var target = Path.GetFullPath(Path.Combine(root, relative));
            if (!target.StartsWith(root, StringComparison.Ordinal)) continue;

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            entry.ExtractToFile(target, overwrite: true);
        }
    }
}
