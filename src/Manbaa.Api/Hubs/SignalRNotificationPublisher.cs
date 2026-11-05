using Microsoft.AspNetCore.SignalR;

using Manbaa.Application.Abstractions;

namespace Manbaa.Api.Hubs;

public class SignalRNotificationPublisher(IHubContext<NotificationsHub> hub, ILogger<SignalRNotificationPublisher> logger) : INotificationPublisher
{
    public async Task PublishAsync(IReadOnlyCollection<NotificationPush> pushes, CancellationToken cancellationToken = default)
    {
        foreach (var push in pushes)
        {
            try
            {
                await hub.Clients.Group(NotificationsHub.UserGroup(push.UserId)).SendAsync(NotificationsHub.ReceiveMethod, push.Notification, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Could not push notification {NotificationId} over SignalR.", push.Notification.Id);
            }
        }
    }
}
