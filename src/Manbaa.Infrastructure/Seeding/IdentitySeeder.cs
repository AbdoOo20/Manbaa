using Microsoft.AspNetCore.Identity;

using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Options;

using Manbaa.Application.Settings;

using Manbaa.Domain.Constants;

using Manbaa.Domain.Entities;

namespace Manbaa.Infrastructure.Seeding;

public static class IdentitySeeder
{
    /// <summary>Creates the roles and the first system admin (from the "SystemAdmin" settings) when they are missing.</summary>
    public static async Task SeedIdentityAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

        try
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            foreach (var roleName in RoleNames.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role(roleName));
                }
            }

            var settings = scope.ServiceProvider.GetRequiredService<IOptions<SystemAdminSettings>>().Value;

            if (string.IsNullOrWhiteSpace(settings.Email) || string.IsNullOrWhiteSpace(settings.Password))
            {
                return;
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            var email = settings.Email.Trim();

            var existing = await userManager.FindByEmailAsync(email);

            if (existing is not null)
            {
                if (existing.TeacherId is not null)
                {
                    logger.LogWarning("The configured system admin email belongs to a teacher's user. Use a different email.");

                    return;
                }

                if (!await userManager.IsInRoleAsync(existing, RoleNames.SystemAdmin))
                {
                    await userManager.AddToRoleAsync(existing, RoleNames.SystemAdmin);
                }

                return;
            }

            var user = new User
            {
                UserName = email,

                Email = email,

                EmailConfirmed = true,

                FullName = settings.FullName
            };

            var created = await userManager.CreateAsync(user, settings.Password);

            if (!created.Succeeded)
            {
                logger.LogWarning("System admin could not be created: {Errors}", string.Join("; ", created.Errors.Select(error => error.Description)));

                return;
            }

            await userManager.AddToRoleAsync(user, RoleNames.SystemAdmin);

            logger.LogInformation("System admin {Email} was created.", email);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Identity seeding was skipped. Make sure the database is migrated.");
        }
    }
}
