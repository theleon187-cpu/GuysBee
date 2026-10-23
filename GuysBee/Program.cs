using System.Globalization;
using GuysBee.Data;
using GuysBee.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=guysbee.db";
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 10;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.User.RequireUniqueEmail = true;

        // Développement local uniquement : mot de passe simple autorisé (ex. compte de test).
        // En production, la politique stricte ci-dessus s'applique.
        if (builder.Environment.IsDevelopment())
        {
            options.Password.RequiredLength = 3;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
        }
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Login";
    options.AccessDeniedPath = "/Admin/Login";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = ctx =>
    {
        var target = ctx.Request.Path.StartsWithSegments("/Admin") ? "/Admin/Login" : "/Compte/Connexion";
        var returnUrl = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);
        ctx.Response.Redirect($"{target}?returnUrl={returnUrl}");
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", p => p.RequireRole(DbInitializer.AdminRole));

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
    options.Conventions.AllowAnonymousToAreaPage("Admin", "/Login");
    options.Conventions.AuthorizeFolder("/Compte");
    options.Conventions.AllowAnonymousToPage("/Compte/Connexion");
    options.Conventions.AllowAnonymousToPage("/Compte/Inscription");
});

builder.Services.AddSingleton<IHtmlCleaner, HtmlCleaner>();
builder.Services.AddSingleton<GuysBee.Services.FontLibrary>();
builder.Services.AddScoped<MediaService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.Cookie.Name = ".GuysBee.Panier";
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.IdleTimeout = TimeSpan.FromHours(4);
});
builder.Services.AddScoped<GuysBee.Services.Shop.ShopSettings>();
builder.Services.AddScoped<GuysBee.Services.Shop.CartService>();
builder.Services.AddScoped<GuysBee.Services.Shop.IntegrationSettings>();
builder.Services.AddHttpClient("integrations", c => c.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddScoped<GuysBee.Services.Shop.IntegrationConnector>();

var app = builder.Build();

// Initialisation de la base, des données de départ et du compte administrateur.
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

// Site en français : formats de dates et de nombres (virgule décimale).
var fr = new CultureInfo("fr-FR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(fr),
    SupportedCultures = [fr],
    SupportedUICultures = [fr]
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Fichiers ajoutés à l'exécution (photos optimisées dans wwwroot/media).
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
