using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.SignalR;

using Manbaa.Application.Abstractions;

namespace Manbaa.Api.Hubs;

/// <summary>
/// Live notifications. The browser connects to <see cref="Route" /> with its token (as the access_token query value)
/// and listens for <see cref="ReceiveMethod" />. Each connection joins its own user group.
/// </summary>
[Authorize]
public class NotificationsHub : Hub
{
    public const string Route = "/hubs/notifications";

    public const string ReceiveMethod = "notification";

    public static string UserGroup(int userId) => $"user:{userId}";

    public static string TeacherGroup(int teacherId) => $"teacher:{teacherId}";

    public override async Task OnConnectedAsync()
    {
        var principal = Context.User;

        if (int.TryParse(principal?.FindFirst(AppClaimTypes.UserId)?.Value, out var userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        }

        if (int.TryParse(principal?.FindFirst(AppClaimTypes.TeacherId)?.Value, out var teacherId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, TeacherGroup(teacherId));
        }

        await base.OnConnectedAsync();
    }
}
