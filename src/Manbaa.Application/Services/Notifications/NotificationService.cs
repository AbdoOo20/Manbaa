using Microsoft.EntityFrameworkCore;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Notifications;

using Manbaa.Domain.Entities;

namespace Manbaa.Application.Services.Notifications;

public class NotificationService(IUnitOfWork unitOfWork, IUserAppService currentUser, TimeProvider clock) : INotificationService
{
    public async Task<Result<PagedResult<NotificationDto>>> GetMineAsync(NotificationQuery query, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.RequiredUserId;

        var notifications = unitOfWork.Repository<Notification>().Query().Where(notification => notification.UserId == userId);

        if (query.UnreadOnly)
        {
            notifications = notifications.Where(notification => !notification.IsRead);
        }

        var page = await notifications
            .OrderByDescending(notification => notification.Id)
            .Select(notification => new NotificationDto(notification.Id, notification.Type, notification.Title, notification.Body, notification.Link, notification.IsRead, notification.CreatedAt))
            .ToPagedResultAsync(query.Page, query.PageSize, cancellationToken);

        return Result<PagedResult<NotificationDto>>.Ok(page);
    }

    public async Task<Result<int>> CountUnreadAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.RequiredUserId;

        var count = await unitOfWork.Repository<Notification>().CountAsync(notification => notification.UserId == userId && !notification.IsRead, cancellationToken);

        return Result<int>.Ok(count);
    }

    public async Task<Result> MarkReadAsync(long id, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.RequiredUserId;

        var notification = await unitOfWork.Repository<Notification>().FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (notification is null)
        {
            return Result.NotFound("الإشعار مش موجود.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;

            notification.ReadAt = clock.GetUtcNow().UtcDateTime;

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }

    public async Task<Result> MarkAllReadAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.RequiredUserId;

        var now = clock.GetUtcNow().UtcDateTime;

        await unitOfWork.Repository<Notification>().Query(asNoTracking: false)
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(notification => notification.IsRead, true).SetProperty(notification => notification.ReadAt, now), cancellationToken);

        return Result.Ok();
    }

    public async Task QueueAsync(IEnumerable<int> userIds, int? teacherId, string type, string title, string body, string? link = null, CancellationToken cancellationToken = default)
    {
        var notifications = userIds.Distinct().Select(userId => new Notification
        {
            UserId = userId,

            TeacherId = teacherId,

            Type = type,

            Title = title,

            Body = body,

            Link = link
        }).ToList();

        if (notifications.Count > 0)
        {
            await unitOfWork.Repository<Notification>().AddRangeAsync(notifications, cancellationToken);
        }
    }
}
