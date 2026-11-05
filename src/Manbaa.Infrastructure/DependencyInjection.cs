using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Services.Auth;

using Manbaa.Application.Services.Teachers;

using Manbaa.Domain.Entities;

using Manbaa.Infrastructure.Persistence;

using Manbaa.Infrastructure.Persistence.Interceptors;

using Manbaa.Infrastructure.Persistence.Repositories;

using Manbaa.Infrastructure.Services.Auth;

using Manbaa.Infrastructure.Services.Teachers;

namespace Manbaa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString) =>
        services.AddInfrastructure(options => options.UseSqlServer(connectionString));

    /// <summary>Registers everything with a custom database provider (the tests use SQLite in memory).</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Action<DbContextOptionsBuilder> useDatabase)
    {
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddScoped<AuditTrailInterceptor>();

        services.AddScoped<NotificationDispatchInterceptor>();

        // Order matters: stamp the rows first, then record the audit trail, then push notifications.
        services.AddDbContext<ManbaaDbContext>((provider, options) =>
        {
            useDatabase(options);

            options.AddInterceptors(
                provider.GetRequiredService<AuditableEntityInterceptor>(),
                provider.GetRequiredService<AuditTrailInterceptor>(),
                provider.GetRequiredService<NotificationDispatchInterceptor>());
        });

        services.AddIdentityCore<User>(options =>
        {
            options.User.RequireUniqueEmail = false;

            options.Password.RequiredLength = 8;

            options.Password.RequireNonAlphanumeric = false;

            options.Password.RequireUppercase = false;

            options.Lockout.MaxFailedAccessAttempts = 5;
        })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<ManbaaDbContext>();

        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddMemoryCache();

        services.AddSingleton<ITeacherAccessService, TeacherAccessService>();

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<ITeacherAccountService, TeacherAccountService>();

        return services;
    }
}
