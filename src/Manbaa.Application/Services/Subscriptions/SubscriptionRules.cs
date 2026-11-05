using Manbaa.Application.Settings;

using Manbaa.Domain.Entities;

using Manbaa.Domain.Enums;

namespace Manbaa.Application.Services.Subscriptions;

/// <summary>The single place that decides where a subscription stands. Queries and access checks all use it.</summary>
public static class SubscriptionRules
{
    public static SubscriptionState StateOf(TeacherStatus status, DateTime? endsAt, DateTime now, SubscriptionSettings settings)
    {
        if (status == TeacherStatus.Suspended)
        {
            return SubscriptionState.Suspended;
        }

        if (endsAt is null)
        {
            return SubscriptionState.Expired;
        }

        if (now <= endsAt)
        {
            if (status == TeacherStatus.Trial)
            {
                return SubscriptionState.Trial;
            }

            return endsAt.Value - now <= TimeSpan.FromDays(settings.ExpiryWarningDays) ? SubscriptionState.ExpiringSoon : SubscriptionState.Active;
        }

        return now <= endsAt.Value.AddDays(settings.GraceDays) ? SubscriptionState.InGrace : SubscriptionState.Expired;
    }

    public static bool CanUseSystem(TeacherStatus status, DateTime? endsAt, DateTime now, SubscriptionSettings settings) =>
        StateOf(status, endsAt, now, settings) is not (SubscriptionState.Expired or SubscriptionState.Suspended);

    /// <summary>Whole days left until the end date (0 on the last day, negative once it has passed).</summary>
    public static int? DaysLeft(DateTime? endsAt, DateTime now) => endsAt is null ? null : (int)Math.Floor((endsAt.Value - now).TotalDays);

    /// <summary>The same rules as <see cref="StateOf" />, written as a query so the database does the filtering.</summary>
    public static IQueryable<Teacher> WhereState(this IQueryable<Teacher> teachers, SubscriptionState state, DateTime now, SubscriptionSettings settings)
    {
        var warningEdge = now.AddDays(settings.ExpiryWarningDays);

        var graceEdge = now.AddDays(-settings.GraceDays);

        return state switch
        {
            SubscriptionState.Suspended => teachers.Where(teacher => teacher.Status == TeacherStatus.Suspended),

            SubscriptionState.Trial => teachers.Where(teacher => teacher.Status == TeacherStatus.Trial && teacher.SubscriptionEndsAt >= now),

            SubscriptionState.Active => teachers.Where(teacher => teacher.Status == TeacherStatus.Active && teacher.SubscriptionEndsAt > warningEdge),

            SubscriptionState.ExpiringSoon => teachers.Where(teacher => teacher.Status == TeacherStatus.Active && teacher.SubscriptionEndsAt >= now && teacher.SubscriptionEndsAt <= warningEdge),

            SubscriptionState.InGrace => teachers.Where(teacher => teacher.Status != TeacherStatus.Suspended && teacher.SubscriptionEndsAt < now && teacher.SubscriptionEndsAt >= graceEdge),

            SubscriptionState.Expired => teachers.Where(teacher => teacher.Status != TeacherStatus.Suspended && (teacher.SubscriptionEndsAt == null || teacher.SubscriptionEndsAt < graceEdge)),

            _ => teachers
        };
    }
}
