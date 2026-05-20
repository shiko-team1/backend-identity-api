using Application.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity.Data;

public static class IdentityInitializer
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityInitializer");

        foreach (var role in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleCreated = await roleManager.CreateAsync(new IdentityRole(role));
                if (!roleCreated.Succeeded)
                {
                    logger.LogError(
                        "Role seed failed. Role={Role}, Errors={Errors}",
                        role,
                        FormatErrors(roleCreated.Errors));
                }
            }
        }

        var adminEmail = configuration["Seed:AdminEmail"]?.Trim();
        var adminPassword = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "Admin seed skipped because seed configuration is incomplete. AdminEmailConfigured={AdminEmailConfigured}, AdminPasswordConfigured={AdminPasswordConfigured}",
                !string.IsNullOrWhiteSpace(adminEmail),
                !string.IsNullOrWhiteSpace(adminPassword));

            return;
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var created = await userManager.CreateAsync(admin, adminPassword);
            if (!created.Succeeded)
            {
                logger.LogError(
                    "Admin seed failed to create admin user. AdminEmail={AdminEmail}, Errors={Errors}",
                    adminEmail,
                    FormatErrors(created.Errors));

                return;
            }

            logger.LogInformation("Admin seed created admin user. AdminEmail={AdminEmail}", adminEmail);
        }
        else if (!admin.EmailConfirmed)
        {
            admin.EmailConfirmed = true;
            var confirmed = await userManager.UpdateAsync(admin);
            if (!confirmed.Succeeded)
            {
                logger.LogError(
                    "Admin seed failed to mark admin email as confirmed. AdminEmail={AdminEmail}, Errors={Errors}",
                    adminEmail,
                    FormatErrors(confirmed.Errors));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, RoleNames.Admin))
        {
            var roleAdded = await userManager.AddToRoleAsync(admin, RoleNames.Admin);
            if (!roleAdded.Succeeded)
            {
                logger.LogError(
                    "Admin seed failed to add admin user to role. AdminEmail={AdminEmail}, Role={Role}, Errors={Errors}",
                    adminEmail,
                    RoleNames.Admin,
                    FormatErrors(roleAdded.Errors));
            }
        }
    }

    private static string FormatErrors(IEnumerable<IdentityError> errors)
    {
        return string.Join("; ", errors.Select(error => $"{error.Code}: {error.Description}"));
    }
}
