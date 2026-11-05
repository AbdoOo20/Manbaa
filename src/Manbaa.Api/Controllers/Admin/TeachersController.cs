using Microsoft.AspNetCore.Mvc;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Subscriptions;

using Manbaa.Application.DTOs.Teachers;

using Manbaa.Application.Services.Subscriptions;

using Manbaa.Application.Services.Teachers;

namespace Manbaa.Api.Controllers.Admin;

public class TeachersController(ITeacherService teacherService, ITeacherAccountService teacherAccountService, ISubscriptionService subscriptionService) : AdminControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<TeacherListItemDto>>>> GetAll([FromQuery] TeacherQuery query, CancellationToken cancellationToken)
    {
        return Respond(await teacherService.GetAllAsync(query, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Result<TeacherDetailsDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        return Respond(await teacherService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Result<TeacherDetailsDto>>> Create(CreateTeacherRequest request, CancellationToken cancellationToken)
    {
        return Respond(await teacherAccountService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Result<TeacherDetailsDto>>> Update(int id, UpdateTeacherRequest request, CancellationToken cancellationToken)
    {
        return Respond(await teacherService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpPost("{id:int}/suspend")]
    public async Task<ActionResult<Result>> Suspend(int id, SuspendTeacherRequest request, CancellationToken cancellationToken)
    {
        return Respond(await teacherService.SuspendAsync(id, request, cancellationToken));
    }

    [HttpPost("{id:int}/activate")]
    public async Task<ActionResult<Result>> Activate(int id, CancellationToken cancellationToken)
    {
        return Respond(await teacherService.ActivateAsync(id, cancellationToken));
    }

    /// <summary>Records a cash payment for the teacher and extends the account.</summary>
    [HttpPost("{id:int}/subscriptions")]
    public async Task<ActionResult<Result<SubscriptionDto>>> RecordSubscription(int id, RecordSubscriptionRequest request, CancellationToken cancellationToken)
    {
        return Respond(await subscriptionService.RecordAsync(id, request, cancellationToken));
    }
}
