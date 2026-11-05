using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Manbaa.Application;
using Manbaa.Application.Abstractions;
using Manbaa.Application.Settings;
using Manbaa.Infrastructure;
using Manbaa.Infrastructure.Persistence;
using Manbaa.Infrastructure.Seeding;

namespace Manbaa.Tests.Support;

/// <summary>The real Application and Infrastructure services over an in-memory SQLite database.</summary>
public sealed class TestApp : IAsyncDisposable
{
    private readonly SqliteConnection connection;

    private readonly ServiceProvider provider;

    private TestApp(SqliteConnection connection, ServiceProvider provider)
    {
        this.connection = connection;

        this.provider = provider;
    }

    public TestUser User { get; } = new();

    public TestClock Clock { get; } = new();

    public CapturingPublisher Publisher { get; } = new();

    public static async Task<TestApp> StartAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        TestApp? app = null;

        var services = new ServiceCollection();

        services.AddLogging();

        services.Configure<JwtSettings>(options =>
        {
            options.Key = "test-key-test-key-test-key-test-key-1234";

            options.Issuer = "Manbaa";

            options.Audience = "Manbaa.Clients";
        });

        services.Configure<SubscriptionSettings>(_ => { });

        services.Configure<SystemAdminSettings>(_ => { });

        services.AddApplication();

        services.AddInfrastructure(options => options.UseSqlite(connection));

        services.AddScoped<IUserAppService>(_ => app!.User);

        services.AddSingleton<TimeProvider>(_ => app!.Clock);

        services.AddSingleton<INotificationPublisher>(_ => app!.Publisher);

        app = new TestApp(connection, services.BuildServiceProvider());

        await using (var scope = app.provider.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<ManbaaDbContext>().Database.EnsureCreatedAsync();
        }

        await app.provider.SeedIdentityAsync();

        return app;
    }

    /// <summary>Runs <paramref name="action" /> in a fresh request scope, like one HTTP request.</summary>
    public async Task<T> RunAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        await using var scope = provider.CreateAsyncScope();

        return await action(scope.ServiceProvider);
    }

    public Task RunAsync(Func<IServiceProvider, Task> action) => RunAsync(async services =>
    {
        await action(services);

        return true;
    });

    public async ValueTask DisposeAsync()
    {
        await provider.DisposeAsync();

        await connection.DisposeAsync();
    }
}
