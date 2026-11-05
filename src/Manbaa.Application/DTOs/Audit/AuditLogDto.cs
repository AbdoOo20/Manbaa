namespace Manbaa.Application.DTOs.Audit;

public record AuditLogDto(
    long Id,
    int? TeacherId,
    int? UserId,
    string? UserName,
    string? UserRole,
    string Action,
    string EntityName,
    string? EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt);
