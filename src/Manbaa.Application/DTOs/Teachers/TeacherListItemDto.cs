using Manbaa.Domain.Enums;

namespace Manbaa.Application.DTOs.Teachers;

public record TeacherListItemDto(
    int Id,
    string FullName,
    string Subject,
    string Phone,
    string? PlanName,
    TeacherStatus Status,
    SubscriptionState State,
    DateTime? SubscriptionEndsAt,
    int? DaysLeft,
    DateTime CreatedAt);
