using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Caching.Memory;

using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Options;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Services.Subscriptions;

using Manbaa.Application.Settings;

using Manbaa.Infrastructure.Persistence;

namespace Manbaa.Infrastructure.Services.Auth;

public class TeacherAccessService(IMemoryCache cache, IServiceScopeFactory scopeFactory, IOptions<SubscriptionSettings> settingsOptions, TimeProvider clock) : ITeacherAccessService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public async Task<bool> IsActiveAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey(teacherId), out bool cached))
        {
            return cached;
        }

        using var scope = scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ManbaaDbContext>();

        var teacher = await context.Teachers
            .AsNoTracking()
            .Where(item => item.Id == teacherId)
            .Select(item => new { item.Status, item.SubscriptionEndsAt })
            .FirstOrDefaultAsync(cancellationToken);

        var isActive = teacher is not null && SubscriptionRules.CanUseSystem(teacher.Status, teacher.SubscriptionEndsAt, clock.GetUtcNow().UtcDateTime, settingsOptions.Value);

        cache.Set(CacheKey(teacherId), isActive, CacheDuration);

        return isActive;
    }

    public void Forget(int teacherId)
    {
        cache.Remove(CacheKey(teacherId));
    }

    private static string CacheKey(int teacherId) => $"teacher-active:{teacherId}";
}
