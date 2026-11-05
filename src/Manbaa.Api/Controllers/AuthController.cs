using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.RateLimiting;

using Manbaa.Api.RateLimiting;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Auth;

using Manbaa.Application.Services.Auth;

namespace Manbaa.Api.Controllers;

public class AuthController(IAuthService authService, IUserAppService userAppService) : ApiControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("login")]
    public async Task<ActionResult<Result<AuthResponse>>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return Respond(await authService.LoginAsync(request, cancellationToken));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<Result<CurrentUserDto>> Me()
    {
        return Respond(Result<CurrentUserDto>.Ok(new CurrentUserDto(userAppService.UserId, userAppService.TeacherId, userAppService.UserName, userAppService.Role)));
    }
}
