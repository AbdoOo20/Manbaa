using System.Linq.Expressions;

using Manbaa.Application.DTOs.Subscriptions;

using Manbaa.Domain.Entities;

namespace Manbaa.Application.Mappings;

public static class SubscriptionMappings
{
    public static readonly Expression<Func<TeacherSubscription, SubscriptionDto>> ToDto = subscription => new SubscriptionDto(
        subscription.Id,
        "S-" + subscription.Id.ToString().PadLeft(6, '0'),
        subscription.TeacherId,
        subscription.Teacher != null ? subscription.Teacher.FullName : null,
        subscription.PlanId,
        subscription.Plan != null ? subscription.Plan.Name : null,
        subscription.Months,
        subscription.Amount,
        subscription.StartsAt,
        subscription.EndsAt,
        subscription.Notes,
        subscription.IsCancelled,
        subscription.CancelReason,
        subscription.CancelledAt,
        subscription.CreatedAt);
}
