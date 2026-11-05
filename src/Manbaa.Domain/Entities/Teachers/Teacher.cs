using Manbaa.Domain.Common;

using Manbaa.Domain.Enums;

namespace Manbaa.Domain.Entities;

/// <summary>The tenant: one teacher (or center) with their own students, staff and content.</summary>
public class Teacher : BaseEntity<int>, ISearchable
{
    public string FullName { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Notes { get; set; }

    public TeacherStatus Status { get; set; } = TeacherStatus.Trial;

    public int? PlanId { get; set; }

    public DateTime? TrialEndsAt { get; set; }

    /// <summary>The last day the teacher may use the system (trial or paid). Recalculated whenever a subscription is recorded or cancelled.</summary>
    public DateTime? SubscriptionEndsAt { get; set; }

    public string? SuspensionReason { get; set; }

    /// <summary>When the "your subscription ends soon" notice was last sent, so it is sent once per period.</summary>
    public DateTime? ExpiryReminderSentAt { get; set; }

    public string SearchText { get; set; } = string.Empty;

    public Plan? Plan { get; set; }

    public ICollection<TeacherSubscription> Subscriptions { get; set; } = [];

    public string BuildSearchText() => ArabicSearch.Normalize(string.Join(' ', FullName, Subject, Phone, Email));
}
