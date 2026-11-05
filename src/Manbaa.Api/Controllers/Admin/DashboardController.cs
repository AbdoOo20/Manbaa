using Microsoft.AspNetCore.Mvc;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Dashboard;

using Manbaa.Application.Services.Dashboard;

namespace Manbaa.Api.Controllers.Admin;

public class DashboardController(IAdminDashboardService dashboardService) : AdminControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<AdminDashboardDto>>> Get(CancellationToken cancellationToken)
    {
        return Respond(await dashboardService.GetAsync(cancellationToken));
    }
}
