using System.Globalization;

using System.Threading.RateLimiting;

using Manbaa.Api.RateLimiting;

using Manbaa.Application.Common;

namespace Manbaa.Api.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimitPolicies.Public, context => PerIp(context, 120));

            options.AddPolicy(RateLimitPolicies.Auth, context => PerIp(context, 20));


            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                await context.HttpContext.Response.WriteResultAsync(Result.TooManyRequests(), cancellationToken);
            };
        });

        return services;
    }

    private static RateLimitPartition<string> PerIp(HttpContext context, int permitLimit)
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,

            Window = TimeSpan.FromMinutes(1),

            QueueLimit = 0,

            AutoReplenishment = true
        });
    }
}
