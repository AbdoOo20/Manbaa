namespace Manbaa.Application.DTOs.Auth;

public record CurrentUserDto(int? UserId, int? TeacherId, string? FullName, string? Role);
