using Manbaa.Domain.Common;

namespace Manbaa.Domain.Entities;

/// <summary>
/// One recorded action: who did it, when, from where, and the values before and after.
/// Written only by the system and never changed or deleted, even by the system admin.
/// </summary>
[NotAudited]
public class AuditLog
{
    public long Id { get; set; }

    public int? TeacherId { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string? UserRole { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }
}
