using System.Security.Claims;

using System.Text;

using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Options;

using Microsoft.IdentityModel.JsonWebTokens;

using Microsoft.IdentityModel.Tokens;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Auth;

using Manbaa.Application.Services.Auth;

using Manbaa.Application.Settings;

using Manbaa.Domain.Constants;

using Manbaa.Domain.Entities;

namespace Manbaa.Infrastructure.Services.Auth;

public class AuthService(
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
    IUserAppService currentUser,
    ITeacherAccessService teacherAccess,
    IOptions<JwtSettings> jwtOptions,
    TimeProvider clock) : IAuthService
{
    private const string InvalidCredentials = "بيانات الدخول غلط.";

    private readonly JwtSettings jwt = jwtOptions.Value;

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var login = request.Login.Trim();

        var user = await FindUserAsync(login, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Result<AuthResponse>.Unauthorized(InvalidCredentials);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result<AuthResponse>.Forbidden("الحساب اتقفل مؤقتاً بسبب محاولات دخول غلط كتير. حاول بعد شوية.");
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);

            await LogAsync(user, AuditActions.LoginFailed, cancellationToken);

            return Result<AuthResponse>.Unauthorized(InvalidCredentials);
        }

        if (user.TeacherId is { } teacherId && !await teacherAccess.IsActiveAsync(teacherId, cancellationToken))
        {
            return Result<AuthResponse>.Forbidden(ITeacherAccessService.NotActiveMessage);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        user.LastLoginAt = clock.GetUtcNow().UtcDateTime;

        await userManager.UpdateAsync(user);

        await LogAsync(user, AuditActions.Login, cancellationToken);

        return Result<AuthResponse>.Ok(await BuildResponseAsync(user));
    }

    /// <summary>Finds the user by email, or by phone when exactly one account uses that phone.</summary>
    private async Task<User?> FindUserAsync(string login, CancellationToken cancellationToken)
    {
        if (login.Contains('@'))
        {
            return await userManager.FindByEmailAsync(login);
        }

        var matches = await userManager.Users.Where(user => user.PhoneNumber == login).Take(2).ToListAsync(cancellationToken);

        return matches.Count == 1 ? matches[0] : null;
    }

    private async Task LogAsync(User user, string action, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);

        await unitOfWork.Repository<AuditLog>().AddAsync(new AuditLog
        {
            TeacherId = user.TeacherId,

            UserId = user.Id,

            UserName = user.FullName,

            UserRole = roles.FirstOrDefault(),

            Action = action,

            EntityName = nameof(User),

            EntityId = user.Id.ToString(),

            IpAddress = currentUser.IpAddress,

            UserAgent = currentUser.UserAgent is { Length: > 512 } agent ? agent[..512] : currentUser.UserAgent,

            CreatedAt = clock.GetUtcNow().UtcDateTime
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> BuildResponseAsync(User user)
    {
        var roles = await userManager.GetRolesAsync(user);

        var role = roles.FirstOrDefault() ?? string.Empty;

        var expiresAt = clock.GetUtcNow().UtcDateTime.AddMinutes(jwt.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(AppClaimTypes.UserId, user.Id.ToString()),

            new(AppClaimTypes.Name, user.FullName),

            new(AppClaimTypes.Role, role)
        };

        if (user.TeacherId is { } teacherId)
        {
            claims.Add(new Claim(AppClaimTypes.TeacherId, teacherId.ToString()));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),

            Issuer = jwt.Issuer,

            Audience = jwt.Audience,

            Expires = expiresAt,

            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new AuthResponse(token, expiresAt, user.Id, user.TeacherId, user.FullName, role);
    }
}
