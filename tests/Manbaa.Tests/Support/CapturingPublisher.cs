using Manbaa.Application.Abstractions;

namespace Manbaa.Tests.Support;

public sealed class CapturingPublisher : INotificationPublisher
{
    public List<NotificationPush> Pushed { get; } = [];

    public Task PublishAsync(IReadOnlyCollection<NotificationPush> pushes, CancellationToken cancellationToken = default)
    {
        Pushed.AddRange(pushes);

        return Task.CompletedTask;
    }
}
