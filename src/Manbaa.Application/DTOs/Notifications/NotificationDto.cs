namespace Manbaa.Application.DTOs.Notifications;

public record NotificationDto(long Id, string Type, string Title, string Body, string? Link, bool IsRead, DateTime CreatedAt);
