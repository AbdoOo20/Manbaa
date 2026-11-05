using Microsoft.AspNetCore.Mvc;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Subscriptions;

using Manbaa.Application.Services.Subscriptions;

namespace Manbaa.Api.Controllers.Admin;

public class SubscriptionsController(ISubscriptionService subscriptionService) : AdminControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<SubscriptionDto>>>> GetAll([FromQuery] SubscriptionQuery query, CancellationToken cancellationToken)
    {
        return Respond(await subscriptionService.GetAllAsync(query, cancellationToken));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<Result<SubscriptionDto>>> Cancel(int id, CancelSubscriptionRequest request, CancellationToken cancellationToken)
    {
        return Respond(await subscriptionService.CancelAsync(id, request, cancellationToken));
    }
}
