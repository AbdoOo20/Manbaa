using System.ComponentModel.DataAnnotations;

namespace Manbaa.Application.DTOs.Subscriptions;

/// <summary>A cash payment for <see cref="Months" /> months. <see cref="Amount" /> defaults to the plan price times the months.</summary>
public record RecordSubscriptionRequest(
    [Range(1, int.MaxValue, ErrorMessage = "اختار الباقة.")] int PlanId,
    [Range(1, 24, ErrorMessage = "عدد الشهور لازم يكون من 1 لـ 24.")] int Months,
    [Range(0, 10_000_000, ErrorMessage = "المبلغ مش صحيح.")] decimal? Amount,
    [MaxLength(500)] string? Notes);
