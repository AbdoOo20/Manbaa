using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Notifications;

namespace Manbaa.Application.Services.Notifications;

public interface INotificationService
{
    Task<Result<PagedResult<NotificationDto>>> GetMineAsync(NotificationQuery query, CancellationToken cancellationToken = default);

    Task<Result<int>> CountUnreadAsync(CancellationToken cancellationToken = default);

    Task<Result> MarkReadAsync(long id, CancellationToken cancellationToken = default);

    Task<Result> MarkAllReadAsync(CancellationToken cancellationToken = default);

    /// <summary>Queues one notification per user. They are saved (and pushed live) with the caller's next save.</summary>
    Task QueueAsync(IEnumerable<int> userIds, int? teacherId, string type, string title, string body, string? link = null, CancellationToken cancellationToken = default);
}
