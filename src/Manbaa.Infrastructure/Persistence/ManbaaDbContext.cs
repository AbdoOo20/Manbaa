using System.Linq.Expressions;

using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

using Manbaa.Application.Abstractions;

using Manbaa.Domain.Common;

using Manbaa.Domain.Entities;

namespace Manbaa.Infrastructure.Persistence;

public class ManbaaDbContext(DbContextOptions<ManbaaDbContext> options, IUserAppService currentUser) : IdentityDbContext<User, Role, int>(options)
{
    public const string SoftDeleteFilter = "SoftDelete";

    public const string TenantFilter = "Tenant";

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<Plan> Plans => Set<Plan>();

    public DbSet<TeacherSubscription> TeacherSubscriptions => Set<TeacherSubscription>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Notification> Notifications => Set<Notification>();

    /// <summary>Read by the tenant query filter on every query, so each request only sees its own teacher's rows.</summary>
    public int? CurrentTeacherId => currentUser.TeacherId;

    /// <summary>The system admin and background jobs (no signed-in user) see every teacher's rows.</summary>
    public bool SeesAllTenants => currentUser.IsSystemAdmin || !currentUser.IsAuthenticated;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("Users");

        modelBuilder.Entity<Role>().ToTable("Roles");

        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");

        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");

        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");

        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManbaaDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(type => type.BaseType is null))
        {
            var clrType = entityType.ClrType;

            var parameter = Expression.Parameter(clrType, "entity");

            if (typeof(ISoftDeletable).IsAssignableFrom(clrType))
            {
                var isNotDeleted = Expression.Equal(Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted)), Expression.Constant(false));

                modelBuilder.Entity(clrType).HasQueryFilter(SoftDeleteFilter, Expression.Lambda(isNotDeleted, parameter));
            }

            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
            {
                var context = Expression.Constant(this);

                var seesAll = Expression.Property(context, nameof(SeesAllTenants));

                var sameTeacher = Expression.Equal(
                    Expression.Convert(Expression.Property(parameter, nameof(ITenantEntity.TeacherId)), typeof(int?)),
                    Expression.Property(context, nameof(CurrentTeacherId)));

                modelBuilder.Entity(clrType).HasQueryFilter(TenantFilter, Expression.Lambda(Expression.OrElse(seesAll, sameTeacher), parameter));
            }
        }
    }
}
