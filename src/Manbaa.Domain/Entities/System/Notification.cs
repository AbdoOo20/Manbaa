using Manbaa.Domain.Common;

namespace Manbaa.Domain.Entities;

/// <summary>An in-app notification for one user, pushed live over SignalR as soon as it is saved.</summary>
[NotAudited]
public class Notification : BaseEntity<long>
{
    public int UserId { get; set; }

    /// <summary>The teacher (tenant) the notification is about, kept for filtering and cleanup. Null for system-wide notices.</summary>
    public int? TeacherId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? Link { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }
}
