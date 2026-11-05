namespace Manbaa.Application.DTOs.Auth;

public record AuthResponse(string Token, DateTime ExpiresAt, int UserId, int? TeacherId, string FullName, string Role);
