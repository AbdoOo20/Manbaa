using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Subscriptions;

using Manbaa.Application.Services.Subscriptions;

using Manbaa.Domain.Constants;

namespace Manbaa.Api.Controllers;

/// <summary>The signed-in teacher's own subscription.</summary>
[Authorize(Roles = RoleNames.Teacher)]
public class SubscriptionController(ISubscriptionService subscriptionService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<MySubscriptionDto>>> GetMine(CancellationToken cancellationToken)
    {
        return Respond(await subscriptionService.GetMineAsync(cancellationToken));
    }
}
