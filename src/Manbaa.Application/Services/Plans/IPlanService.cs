using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Plans;

namespace Manbaa.Application.Services.Plans;

public interface IPlanService
{
    Task<Result<IReadOnlyList<PlanDto>>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default);

    Task<Result<PlanDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<PlanDto>> CreateAsync(SavePlanRequest request, CancellationToken cancellationToken = default);

    Task<Result<PlanDto>> UpdateAsync(int id, SavePlanRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
