using System.Security.Claims;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Exceptions;

using Manbaa.Domain.Constants;

namespace Manbaa.Api.Services;

public class UserAppService(IHttpContextAccessor httpContextAccessor) : IUserAppService
{
    private HttpContext? HttpContext => httpContextAccessor.HttpContext;

    private ClaimsPrincipal? Principal => HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public int? UserId => ReadInt(AppClaimTypes.UserId) ?? ReadInt(ClaimTypes.NameIdentifier);

    public int? TeacherId => ReadInt(AppClaimTypes.TeacherId);

    public string? UserName => Principal?.FindFirst(AppClaimTypes.Name)?.Value;

    public string? Role => Principal?.FindFirst(AppClaimTypes.Role)?.Value ?? Principal?.FindFirst(ClaimTypes.Role)?.Value;

    public bool IsSystemAdmin => IsAuthenticated && Principal!.IsInRole(RoleNames.SystemAdmin);

    public string? IpAddress => HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => HttpContext?.Request.Headers.UserAgent.ToString() is { Length: > 0 } agent ? agent : null;

    public int RequiredUserId => UserId ?? throw new UnauthorizedAccessException("لازم تسجّل دخول الأول.");

    public int RequiredTeacherId => TeacherId ?? throw new ForbiddenException("الحساب ده مش تابع لأي مدرس.");

    private int? ReadInt(string claimType)
    {
        var value = Principal?.FindFirst(claimType)?.Value;

        return int.TryParse(value, out var result) ? result : null;
    }
}
