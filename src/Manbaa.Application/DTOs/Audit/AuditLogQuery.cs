using Manbaa.Application.Common;

namespace Manbaa.Application.DTOs.Audit;

public class AuditLogQuery : PagedQuery
{
    /// <summary>Only used by the system admin; a teacher always sees their own logs.</summary>
    public int? TeacherId { get; set; }

    public int? UserId { get; set; }

    public string? EntityName { get; set; }

    public string? EntityId { get; set; }

    public string? Action { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }
}
