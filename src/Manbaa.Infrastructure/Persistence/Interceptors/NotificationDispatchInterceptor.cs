using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Diagnostics;

using Manbaa.Application.Abstractions;

using Manbaa.Application.DTOs.Notifications;

using Manbaa.Domain.Entities;

namespace Manbaa.Infrastructure.Persistence.Interceptors;

/// <summary>Pushes every newly saved <see cref="Notification" /> to its user over SignalR, right after the save succeeds.</summary>
public class NotificationDispatchInterceptor(INotificationPublisher publisher) : SaveChangesInterceptor
{
    private List<Notification> added = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context)
        {
            added.AddRange(context.ChangeTracker.Entries<Notification>().Where(entry => entry.State == EntityState.Added).Select(entry => entry.Entity));
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (added.Count > 0)
        {
            var pushes = added
                .Select(item => new NotificationPush(item.UserId, new NotificationDto(item.Id, item.Type, item.Title, item.Body, item.Link, item.IsRead, item.CreatedAt)))
                .ToList();

            added = [];

            await publisher.PublishAsync(pushes, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        added = [];

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }
}
