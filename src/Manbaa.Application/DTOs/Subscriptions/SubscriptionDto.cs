namespace Manbaa.Application.DTOs.Subscriptions;

public record SubscriptionDto(
    int Id,
    string ReceiptNumber,
    int TeacherId,
    string? TeacherName,
    int PlanId,
    string? PlanName,
    int Months,
    decimal Amount,
    DateTime StartsAt,
    DateTime EndsAt,
    string? Notes,
    bool IsCancelled,
    string? CancelReason,
    DateTime? CancelledAt,
    DateTime CreatedAt);
