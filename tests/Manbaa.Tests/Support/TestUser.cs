using Manbaa.Application.Abstractions;
using Manbaa.Application.Exceptions;
using Manbaa.Domain.Constants;

namespace Manbaa.Tests.Support;

/// <summary>The "signed-in" user for a test. Switch it between steps with the As* methods.</summary>
public sealed class TestUser : IUserAppService
{
    public bool IsAuthenticated { get; private set; }

    public int? UserId { get; private set; }

    public int? TeacherId { get; private set; }

    public string? UserName { get; private set; }

    public string? Role { get; private set; }

    public bool IsSystemAdmin => Role == RoleNames.SystemAdmin;

    public string? IpAddress => "10.0.0.1";

    public string? UserAgent => "tests";

    public int RequiredUserId => UserId ?? throw new UnauthorizedAccessException();

    public int RequiredTeacherId => TeacherId ?? throw new ForbiddenException();

    public void AsAdmin(int userId = 1) => Set(userId, null, RoleNames.SystemAdmin, "Admin");

    public void AsTeacher(int teacherId, int userId) => Set(userId, teacherId, RoleNames.Teacher, $"Teacher {teacherId}");

    public void AsSystemJob() => Set(null, null, null, null, authenticated: false);

    private void Set(int? userId, int? teacherId, string? role, string? name, bool authenticated = true)
    {
        IsAuthenticated = authenticated;

        UserId = userId;

        TeacherId = teacherId;

        Role = role;

        UserName = name;
    }
}
