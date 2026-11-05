using Manbaa.Application.Common;

namespace Manbaa.Application.DTOs.Notifications;

public class NotificationQuery : PagedQuery
{
    public bool UnreadOnly { get; set; }
}
