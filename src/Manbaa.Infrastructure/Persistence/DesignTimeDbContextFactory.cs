using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Design;

using Manbaa.Application.Abstractions;

namespace Manbaa.Infrastructure.Persistence;

/// <summary>
/// Lets "dotnet ef migrations add" build the model without starting the API.
/// "dotnet ef database update" takes the real server with --connection "...".
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ManbaaDbContext>
{
    public ManbaaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ManbaaDbContext>()
            .UseSqlServer("Server=localhost;Database=Manbaa;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        return new ManbaaDbContext(options, new DesignTimeUser());
    }

    private sealed class DesignTimeUser : IUserAppService
    {
        public bool IsAuthenticated => false;

        public int? UserId => null;

        public int? TeacherId => null;

        public string? UserName => null;

        public string? Role => null;

        public bool IsSystemAdmin => false;

        public string? IpAddress => null;

        public string? UserAgent => null;

        public int RequiredUserId => throw new InvalidOperationException();

        public int RequiredTeacherId => throw new InvalidOperationException();
    }
}
