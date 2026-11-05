using Manbaa.Application.DTOs.Notifications;

namespace Manbaa.Application.Abstractions;

public record NotificationPush(int UserId, NotificationDto Notification);

/// <summary>Pushes saved notifications to connected browsers. Implemented over SignalR in the Api layer.</summary>
public interface INotificationPublisher
{
    Task PublishAsync(IReadOnlyCollection<NotificationPush> pushes, CancellationToken cancellationToken = default);
}
