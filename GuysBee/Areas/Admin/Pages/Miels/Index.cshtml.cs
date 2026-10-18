using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Areas.Admin.Pages.Miels;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Product> Products { get; private set; } = new();
    public List<MediaAsset> Medias { get; private set; } = new();

    public List<Category> Categories { get; private set; } = new();

    public async Task<IActionResult> OnPostSaveCategoryAsync(int id, string name, string? description, string? menuLabel)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Le libellé de la gamme est obligatoire.";
            return RedirectToPage();
        }
        c.Name = name.Trim();
        c.Description = description?.Trim();
        c.MenuLabel = string.IsNullOrWhiteSpace(menuLabel) ? null : menuLabel.Trim();
        await _db.SaveChangesAsync();
        await SyncMenuAsync(c);
        TempData["Message"] = $"Gamme « {c.Name} » enregistrée.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveCategoryAsync(int id, int direction)
    {
        var list = await _db.Categories.OrderBy(c => c.SortOrder).ThenBy(c => c.Id).ToListAsync();
        var i = list.FindIndex(c => c.Id == id);
        var j = i + direction;
        if (i >= 0 && j >= 0 && j < list.Count) (list[i], list[j]) = (list[j], list[i]);
        for (var k = 0; k < list.Count; k++) list[k].SortOrder = k + 1;
        await _db.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteCategoryAsync(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c is null) return RedirectToPage();
        if (await _db.Products.AnyAsync(p => p.CategoryId == id))
        {
            TempData["Error"] = "Cette gamme contient encore des produits : changez leur gamme avant de la supprimer.";
            return RedirectToPage();
        }
        _db.Categories.Remove(c);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Gamme supprimée.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddCategoryMenuAsync(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        var url = "/Produits?gamme=" + c.Slug;
        if (await _db.MenuItems.AnyAsync(m => m.Url == url && !m.IsFooter))
        {
            TempData["Error"] = "Cette gamme est déjà dans le menu.";
            return RedirectToPage();
        }
        var max = await _db.MenuItems.Where(m => !m.IsFooter).Select(m => (int?)m.SortOrder).MaxAsync() ?? 0;
        _db.MenuItems.Add(new MenuItem { Label = c.Name, Url = url, SortOrder = max + 1 });
        await _db.SaveChangesAsync();
        TempData["Message"] = $"« {c.Name} » ajouté au menu. Vous pouvez le déplacer dans Menu du site.";
        return RedirectToPage();
    }

    private async Task SyncMenuAsync(Category c)
    {
        var url = "/Produits?gamme=" + c.Slug;
        var item = await _db.MenuItems.FirstOrDefaultAsync(m => m.Url == url && !m.IsFooter);
        var label = c.MenuLabel?.Trim();
        if (string.IsNullOrEmpty(label))
        {
            if (item is not null) _db.MenuItems.Remove(item);
        }
        else if (item is null)
        {
            var max = await _db.MenuItems.Where(m => !m.IsFooter).Select(m => (int?)m.SortOrder).MaxAsync() ?? 0;
            _db.MenuItems.Add(new MenuItem { Label = label, Url = url, SortOrder = max + 1 });
        }
        else item.Label = label;
        await _db.SaveChangesAsync();
    }

    public async Task<IActionResult> OnPostAddCategoryAsync(string name, string? menuLabel)
    {
        name = name?.Trim() ?? "";
        if (name.Length == 0)
        {
            TempData["Error"] = "Indiquez le nom de la gamme.";
            return RedirectToPage();
        }
        var slug = new string(name.ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');
        if (slug.Length == 0 || await _db.Categories.AnyAsync(c => c.Slug == slug))
        {
            TempData["Error"] = "Cette gamme existe déjà.";
            return RedirectToPage();
        }
        var max = await _db.Categories.Select(c => (int?)c.SortOrder).MaxAsync() ?? 0;
        var cat = new Category { Name = name, Slug = slug, SortOrder = max + 1, MenuLabel = string.IsNullOrWhiteSpace(menuLabel) ? null : menuLabel.Trim() };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        await SyncMenuAsync(cat);
        TempData["Message"] = $"Gamme « {name} » ajoutée.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddProductAsync(string name, string shortDescription, int? categoryId, string? variantLabel, decimal price, int weight)
    {
        name = name?.Trim() ?? "";
        if (name.Length == 0 || string.IsNullOrWhiteSpace(shortDescription) || price < 0 || weight < 0)
        {
            TempData["Error"] = "Le nom, la description courte et un prix valide sont obligatoires.";
            return RedirectToPage();
        }
        var baseSlug = new string(name.ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');
        if (baseSlug.Length == 0) baseSlug = "produit";
        var slug = baseSlug;
        for (var n = 2; await _db.Products.AnyAsync(x => x.Slug == slug); n++) slug = $"{baseSlug}-{n}";
        var max = await _db.Products.Select(x => (int?)x.SortOrder).MaxAsync() ?? 0;
        var p = new Product
        {
            Name = name,
            Slug = slug,
            ShortDescription = shortDescription.Trim(),
            CategoryId = categoryId,
            IsActive = false,
            SortOrder = max + 1
        };
        p.Variants.Add(new ProductVariant
        {
            Label = string.IsNullOrWhiteSpace(variantLabel) ? "Unité" : variantLabel.Trim(),
            Price = price,
            ShippingWeightGrams = weight,
            Stock = 0
        });
        _db.Products.Add(p);
        await _db.SaveChangesAsync();
        TempData["Message"] = $"« {name} » créé (masqué, stock à 0). Complétez-le puis cochez « Visible sur le site ».";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddVariantAsync(int productId, string label, decimal price, int weight)
    {
        if (string.IsNullOrWhiteSpace(label) || price < 0 || weight < 0 || !await _db.Products.AnyAsync(x => x.Id == productId))
        {
            TempData["Error"] = "Indiquez un nom de pot et un prix valide.";
            return RedirectToPage();
        }
        _db.ProductVariants.Add(new ProductVariant { ProductId = productId, Label = label.Trim(), Price = price, ShippingWeightGrams = weight });
        await _db.SaveChangesAsync();
        TempData["Message"] = "Format ajouté.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetCategoryAsync(int id, int? categoryId)
    {
        var p = await _db.Products.FindAsync(id);
        if (p is null) return NotFound();
        p.CategoryId = categoryId;
        await _db.SaveChangesAsync();
        TempData["Message"] = $"Gamme de « {p.Name} » enregistrée.";
        return RedirectToPage();
    }

    public async Task OnGetAsync()
    {
        Categories = await _db.Categories.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync();
        Products = await _db.Products.Include(p => p.Variants).OrderBy(p => p.SortOrder).ToListAsync();
        Medias = await _db.MediaAssets.AsNoTracking().OrderBy(m => m.SortOrder).ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveProductAsync(int id, string name, string shortDescription,
        string? longDescription, string? origin, string? usageTips, bool isActive, int? mainMediaId)
    {
        var p = await _db.Products.FindAsync(id);
        if (p is null) return NotFound();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(shortDescription))
        {
            TempData["Error"] = "Le nom et la description courte sont obligatoires.";
            return RedirectToPage();
        }
        p.Name = name.Trim();
        p.ShortDescription = shortDescription.Trim();
        p.LongDescription = longDescription?.Trim();
        p.Origin = origin?.Trim();
        p.UsageTips = usageTips?.Trim();
        p.IsActive = isActive;
        p.MainMediaId = mainMediaId;
        p.UpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Message"] = $"« {p.Name} » enregistré.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveVariantAsync(int id, decimal price, int stock, int weight, bool isActive)
    {
        var v = await _db.ProductVariants.FindAsync(id);
        if (v is null) return NotFound();
        if (price < 0 || stock < 0 || weight < 0)
        {
            TempData["Error"] = "Les valeurs ne peuvent pas être négatives.";
            return RedirectToPage();
        }
        v.Price = price;
        v.Stock = stock;
        v.ShippingWeightGrams = weight;
        v.IsActive = isActive;
        await _db.SaveChangesAsync();
        TempData["Message"] = $"« {v.Label} » enregistré.";
        return RedirectToPage();
    }
}
