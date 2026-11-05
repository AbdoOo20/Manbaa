using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.ChangeTracking;

using Microsoft.EntityFrameworkCore.Diagnostics;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Exceptions;

using Manbaa.Domain.Common;

namespace Manbaa.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Runs before every save: stamps created/updated by and at, turns deletes into soft deletes,
/// rebuilds search text, fills the teacher of new tenant rows and refuses writing another teacher's rows.
/// </summary>
public class AuditableEntityInterceptor(IUserAppService currentUser, TimeProvider clock) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = clock.GetUtcNow().UtcDateTime;

        var userId = currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            GuardTenant(entry);

            if (entry is { State: EntityState.Deleted, Entity: ISoftDeletable softDeletable })
            {
                entry.State = EntityState.Modified;

                softDeletable.IsDeleted = true;
            }

            if (entry.Entity is ISearchable searchable && entry.State is EntityState.Added or EntityState.Modified)
            {
                var searchText = searchable.BuildSearchText();

                if (searchable.SearchText != searchText)
                {
                    searchable.SearchText = searchText;
                }
            }

            if (entry.Entity is not IAuditable auditable)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                auditable.CreatedAt = now;

                auditable.CreatedBy = userId;
            }

            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;

                entry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;

                auditable.UpdatedAt = now;

                auditable.UpdatedBy = userId;
            }
        }
    }

    private void GuardTenant(EntityEntry entry)
    {
        if (entry.Entity is not ITenantEntity tenantEntity || currentUser.IsSystemAdmin || !currentUser.IsAuthenticated)
        {
            return;
        }

        var teacherId = currentUser.TeacherId ?? throw new ForbiddenException("الحساب ده مش تابع لأي مدرس.");

        if (entry.State == EntityState.Added && tenantEntity.TeacherId == 0)
        {
            tenantEntity.TeacherId = teacherId;
        }

        var original = entry.State == EntityState.Added ? tenantEntity.TeacherId : (int)entry.Property(nameof(ITenantEntity.TeacherId)).OriginalValue!;

        if (tenantEntity.TeacherId != teacherId || original != teacherId)
        {
            throw new ForbiddenException();
        }
    }
}
