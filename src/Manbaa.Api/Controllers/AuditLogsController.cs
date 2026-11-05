using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Audit;

using Manbaa.Application.Services.Audit;

using Manbaa.Domain.Constants;

namespace Manbaa.Api.Controllers;

/// <summary>The teacher's own activity log: everything the teacher and their staff did.</summary>
[Authorize(Roles = RoleNames.Teacher)]
public class AuditLogsController(IAuditLogService auditLogService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<AuditLogDto>>>> Get([FromQuery] AuditLogQuery query, CancellationToken cancellationToken)
    {
        return Respond(await auditLogService.GetForTeacherAsync(query, cancellationToken));
    }
}
