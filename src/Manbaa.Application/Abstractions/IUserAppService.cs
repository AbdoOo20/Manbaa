namespace Manbaa.Application.Abstractions;

/// <summary>The signed-in user of the current request. Every value is null outside a request (background jobs).</summary>
public interface IUserAppService
{
    bool IsAuthenticated { get; }

    int? UserId { get; }

    int? TeacherId { get; }

    string? UserName { get; }

    string? Role { get; }

    bool IsSystemAdmin { get; }

    string? IpAddress { get; }

    string? UserAgent { get; }

    int RequiredUserId { get; }

    int RequiredTeacherId { get; }
}
