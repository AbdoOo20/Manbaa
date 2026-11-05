using Manbaa.Domain.Enums;

namespace Manbaa.Application.DTOs.Subscriptions;

/// <summary>What a teacher sees about their own account: plan, limits and how long is left.</summary>
public record MySubscriptionDto(
    string? PlanName,
    int? MaxStudents,
    int? MaxSecretaries,
    int? MaxStorageGb,
    TeacherStatus Status,
    SubscriptionState State,
    DateTime? SubscriptionEndsAt,
    int? DaysLeft,
    IReadOnlyList<SubscriptionDto> History);
