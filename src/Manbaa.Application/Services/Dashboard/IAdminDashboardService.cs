using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Dashboard;

namespace Manbaa.Application.Services.Dashboard;

public interface IAdminDashboardService
{
    Task<Result<AdminDashboardDto>> GetAsync(CancellationToken cancellationToken = default);
}
