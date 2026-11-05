using Manbaa.Application.DTOs.Teachers;

using Manbaa.Application.Services.Subscriptions;

using Manbaa.Application.Settings;

using Manbaa.Domain.Entities;

using Manbaa.Domain.Enums;

namespace Manbaa.Application.Mappings;

public static class TeacherMappings
{
    /// <summary>The columns a teacher list needs, read from the database before the state is calculated in memory.</summary>
    public record TeacherRow(int Id, string FullName, string Subject, string Phone, string? PlanName, TeacherStatus Status, DateTime? SubscriptionEndsAt, DateTime CreatedAt);

    public static IQueryable<TeacherRow> SelectRows(this IQueryable<Teacher> teachers) => teachers.Select(teacher => new TeacherRow(
        teacher.Id,
        teacher.FullName,
        teacher.Subject,
        teacher.Phone,
        teacher.Plan != null ? teacher.Plan.Name : null,
        teacher.Status,
        teacher.SubscriptionEndsAt,
        teacher.CreatedAt));

    public static TeacherListItemDto ToListItem(this TeacherRow row, DateTime now, SubscriptionSettings settings) => new(
        row.Id,
        row.FullName,
        row.Subject,
        row.Phone,
        row.PlanName,
        row.Status,
        SubscriptionRules.StateOf(row.Status, row.SubscriptionEndsAt, now, settings),
        row.SubscriptionEndsAt,
        SubscriptionRules.DaysLeft(row.SubscriptionEndsAt, now),
        row.CreatedAt);
}
