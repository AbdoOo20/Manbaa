using Manbaa.Domain.Common;

namespace Manbaa.Domain.Entities;

/// <summary>
/// One paid period, collected in cash by the system admin. Never edited or deleted:
/// a mistake is corrected by cancelling it with a reason, which keeps the record.
/// </summary>
public class TeacherSubscription : BaseEntity<int>, ITenantEntity
{
    public int TeacherId { get; set; }

    public int PlanId { get; set; }

    public int Months { get; set; }

    public decimal Amount { get; set; }

    public DateTime StartsAt { get; set; }

    public DateTime EndsAt { get; set; }

    public string? Notes { get; set; }

    public bool IsCancelled { get; set; }

    public string? CancelReason { get; set; }

    public DateTime? CancelledAt { get; set; }

    public int? CancelledBy { get; set; }

    public Teacher? Teacher { get; set; }

    public Plan? Plan { get; set; }

    /// <summary>The number printed on the cash receipt.</summary>
    public string ReceiptNumber => $"S-{Id:D6}";
}
