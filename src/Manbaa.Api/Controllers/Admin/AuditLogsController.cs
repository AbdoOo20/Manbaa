using Microsoft.AspNetCore.Mvc;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Audit;

using Manbaa.Application.Services.Audit;

namespace Manbaa.Api.Controllers.Admin;

public class AuditLogsController(IAuditLogService auditLogService) : AdminControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<AuditLogDto>>>> Get([FromQuery] AuditLogQuery query, CancellationToken cancellationToken)
    {
        return Respond(await auditLogService.GetForAdminAsync(query, cancellationToken));
    }
}
