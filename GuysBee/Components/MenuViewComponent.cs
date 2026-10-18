using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Components;

public class MenuViewComponent : ViewComponent
{
    private readonly AppDbContext _db;
    public MenuViewComponent(AppDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync(bool footer = false)
    {
        var items = await _db.MenuItems.AsNoTracking()
            .Where(m => m.IsVisible && m.IsFooter == footer)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();
        return View(items);
    }
}
