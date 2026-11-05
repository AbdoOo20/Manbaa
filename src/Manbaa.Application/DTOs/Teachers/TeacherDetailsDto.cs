using Manbaa.Application.DTOs.Subscriptions;

using Manbaa.Domain.Enums;

namespace Manbaa.Application.DTOs.Teachers;

public record TeacherDetailsDto(
    int Id,
    string FullName,
    string Subject,
    string Phone,
    string? Email,
    string? Notes,
    int? PlanId,
    string? PlanName,
    TeacherStatus Status,
    SubscriptionState State,
    DateTime? SubscriptionEndsAt,
    int? DaysLeft,
    string? SuspensionReason,
    DateTime CreatedAt,
    IReadOnlyList<SubscriptionDto> Subscriptions);
