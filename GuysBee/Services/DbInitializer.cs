using System.Security.Cryptography;
using GuysBee.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Services;

public static class DbInitializer
{
    public const string AdminRole = "Admin";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        // Tant qu'aucune migration n'existe, on crée le schéma directement.
        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        await DbSeeder.SeedAsync(db);
        await ContentSeeder.SeedAsync(db);
        await SeedAdminAsync(services, logger);
    }

    private static async Task SeedAdminAsync(IServiceProvider services, ILogger logger)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<IdentityUser>>();
        var config = services.GetRequiredService<IConfiguration>();

        if (!await roles.RoleExistsAsync(AdminRole))
            await roles.CreateAsync(new IdentityRole(AdminRole));

        if ((await users.GetUsersInRoleAsync(AdminRole)).Count > 0) return;

        var email = config["Admin:Email"] ?? "admin@guysbee.local";
        var password = config["Admin:Password"];
        var generated = false;
        if (string.IsNullOrWhiteSpace(password))
        {
            password = "Gb-" + Convert.ToBase64String(RandomNumberGenerator.GetBytes(9))
                .Replace('+', 'a').Replace('/', 'b') + "9!";
            generated = true;
        }

        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogError("Création de l'administrateur impossible : {Errors}",
                string.Join(" ; ", result.Errors.Select(e => e.Description)));
            return;
        }
        await users.AddToRoleAsync(user, AdminRole);

        if (generated)
            logger.LogWarning("Compte administrateur créé. Identifiant : {Email} | Mot de passe temporaire : {Password} (à changer dès la première connexion).", email, password);
        else
            logger.LogInformation("Compte administrateur créé pour {Email}.", email);
    }
}
