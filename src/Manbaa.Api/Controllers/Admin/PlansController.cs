using Microsoft.AspNetCore.Mvc;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Plans;

using Manbaa.Application.Services.Plans;

namespace Manbaa.Api.Controllers.Admin;

public class PlansController(IPlanService planService) : AdminControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<IReadOnlyList<PlanDto>>>> GetAll([FromQuery] bool activeOnly, CancellationToken cancellationToken)
    {
        return Respond(await planService.GetAllAsync(activeOnly, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result<PlanDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        return Respond(await planService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Result<PlanDto>>> Create(SavePlanRequest request, CancellationToken cancellationToken)
    {
        return Respond(await planService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Result<PlanDto>>> Update(int id, SavePlanRequest request, CancellationToken cancellationToken)
    {
        return Respond(await planService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<Result>> Delete(int id, CancellationToken cancellationToken)
    {
        return Respond(await planService.DeleteAsync(id, cancellationToken));
    }
}
