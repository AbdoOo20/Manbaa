using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Notifications;

using Manbaa.Application.Services.Notifications;

namespace Manbaa.Api.Controllers;

[Authorize]
public class NotificationsController(INotificationService notificationService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<NotificationDto>>>> GetMine([FromQuery] NotificationQuery query, CancellationToken cancellationToken)
    {
        return Respond(await notificationService.GetMineAsync(query, cancellationToken));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<Result<int>>> CountUnread(CancellationToken cancellationToken)
    {
        return Respond(await notificationService.CountUnreadAsync(cancellationToken));
    }

    [HttpPost("{id:long}/read")]
    public async Task<ActionResult<Result>> MarkRead(long id, CancellationToken cancellationToken)
    {
        return Respond(await notificationService.MarkReadAsync(id, cancellationToken));
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<Result>> MarkAllRead(CancellationToken cancellationToken)
    {
        return Respond(await notificationService.MarkAllReadAsync(cancellationToken));
    }
}
