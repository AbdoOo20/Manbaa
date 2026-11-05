using Manbaa.Application.Common;

using Manbaa.Application.DTOs.Auth;

namespace Manbaa.Application.Services.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
