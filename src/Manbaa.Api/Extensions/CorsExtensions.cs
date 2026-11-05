namespace Manbaa.Api.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "Frontend";

    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (origins.Length > 0)
                {
                    policy.WithOrigins(origins)

                        .AllowAnyHeader()

                        .AllowAnyMethod()

                        .AllowCredentials();
                }
            });
        });

        return services;
    }
}
