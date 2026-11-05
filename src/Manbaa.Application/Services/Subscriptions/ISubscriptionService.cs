using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Subscriptions;

namespace Manbaa.Application.Services.Subscriptions;

public interface ISubscriptionService
{
    Task<Result<PagedResult<SubscriptionDto>>> GetAllAsync(SubscriptionQuery query, CancellationToken cancellationToken = default);

    /// <summary>Records a cash payment and extends the teacher's account.</summary>
    Task<Result<SubscriptionDto>> RecordAsync(int teacherId, RecordSubscriptionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Cancels the teacher's latest payment (kept on record with the reason) and moves the end date back.</summary>
    Task<Result<SubscriptionDto>> CancelAsync(int subscriptionId, CancelSubscriptionRequest request, CancellationToken cancellationToken = default);

    /// <summary>The signed-in teacher's own plan, end date and payment history.</summary>
    Task<Result<MySubscriptionDto>> GetMineAsync(CancellationToken cancellationToken = default);
}
