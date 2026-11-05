using Manbaa.Application.Common;

namespace Manbaa.Application.DTOs.Subscriptions;

public class SubscriptionQuery : PagedQuery
{
    public int? TeacherId { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public bool IncludeCancelled { get; set; }
}
