using System.ComponentModel.DataAnnotations;

namespace Manbaa.Application.DTOs.Subscriptions;

public record CancelSubscriptionRequest([Required(ErrorMessage = "اكتب سبب الإلغاء."), MaxLength(500)] string Reason);
