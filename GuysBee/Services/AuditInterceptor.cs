using GuysBee.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GuysBee.Services;

/// <summary>
/// Historisation automatique : chaque création, modification ou suppression faite
/// par un administrateur connecté est tracée dans AuditEntries.
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private static readonly string[] LabelProperties = { "Title", "Name", "Term", "Key", "Label", "Number", "Subject" };
    private static readonly string[] HiddenFragments = { "Password", "Hash", "Secret", "Token", "Stamp" };
    private const int MaxValueLength = 80;

    private readonly IHttpContextAccessor _http;
    public AuditInterceptor(IHttpContextAccessor http) => _http = http;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Track(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Track(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void Track(DbContext? context)
    {
        var user = _http.HttpContext?.User;
        if (context is null || user?.Identity?.IsAuthenticated != true || !user.IsInRole(DbInitializer.AdminRole))
            return;

        var userName = user.Identity.Name ?? "admin";
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditEntry
                        && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                        && !e.Metadata.ClrType.Namespace!.StartsWith("Microsoft.AspNetCore.Identity", StringComparison.Ordinal))
            .ToList();

        foreach (var e in entries)
        {
            var details = e.State == EntityState.Modified ? Changes(e) : Label(e);
            if (e.State == EntityState.Modified && details.Length == 0)
                continue;

            var key = e.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey() && !p.IsTemporary)?.CurrentValue?.ToString();
            context.Set<AuditEntry>().Add(new AuditEntry
            {
                UserName = userName,
                Action = e.State switch
                {
                    EntityState.Added => "Création",
                    EntityState.Deleted => "Suppression",
                    _ => "Modification"
                },
                EntityName = e.Metadata.ClrType.Name,
                EntityId = key,
                Details = details.Length > 0 ? details : null,
                CreatedUtc = DateTime.UtcNow
            });
        }
    }

    private static string Label(EntityEntry e)
    {
        foreach (var name in LabelProperties)
        {
            var p = e.Properties.FirstOrDefault(x => x.Metadata.Name == name);
            if (p?.CurrentValue is string s && s.Length > 0)
                return Cut(s);
        }
        return "";
    }

    private static string Changes(EntityEntry e)
    {
        var parts = new List<string>();
        foreach (var p in e.Properties.Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue)))
        {
            var name = p.Metadata.Name;
            if (HiddenFragments.Any(h => name.Contains(h, StringComparison.OrdinalIgnoreCase)))
                continue;
            parts.Add($"{name} : {Cut(p.OriginalValue?.ToString())} → {Cut(p.CurrentValue?.ToString())}");
        }
        return string.Join(" ; ", parts);
    }

    private static string Cut(string? v)
    {
        if (string.IsNullOrEmpty(v)) return "(vide)";
        v = v.Replace('\r', ' ').Replace('\n', ' ');
        return v.Length <= MaxValueLength ? v : v[..MaxValueLength] + "…";
    }
}
