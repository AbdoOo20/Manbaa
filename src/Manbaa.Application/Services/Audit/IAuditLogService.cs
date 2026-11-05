using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Audit;

namespace Manbaa.Application.Services.Audit;

public interface IAuditLogService
{
    /// <summary>The signed-in teacher's own log: every action of the teacher and their staff.</summary>
    Task<Result<PagedResult<AuditLogDto>>> GetForTeacherAsync(AuditLogQuery query, CancellationToken cancellationToken = default);

    /// <summary>The whole system's log, optionally narrowed to one teacher.</summary>
    Task<Result<PagedResult<AuditLogDto>>> GetForAdminAsync(AuditLogQuery query, CancellationToken cancellationToken = default);
}
