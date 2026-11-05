using System.Reflection;

using System.Text.Json;

using System.Text.Encodings.Web;

using System.Text.Unicode;

using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.ChangeTracking;

using Microsoft.EntityFrameworkCore.Diagnostics;

using Microsoft.EntityFrameworkCore.Metadata;

using Microsoft.EntityFrameworkCore.Storage;

using Manbaa.Application.Abstractions;

using Manbaa.Domain.Common;

using Manbaa.Domain.Constants;

using Manbaa.Domain.Entities;

namespace Manbaa.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Writes an <see cref="AuditLog" /> row for every create, update and delete of every entity (except the ones marked
/// <see cref="NotAuditedAttribute" />), in the same transaction as the change. Audit rows can never be updated or deleted.
/// </summary>
public class AuditTrailInterceptor(IUserAppService currentUser, TimeProvider clock) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };

    private static readonly HashSet<Type> SkippedTypes = [typeof(IdentityUserToken<int>), typeof(IdentityUserLogin<int>)];

    private static readonly HashSet<string> IgnoredProperties =
    [
        nameof(ISearchable.SearchText),

        nameof(IAuditable.CreatedAt),

        nameof(IAuditable.CreatedBy),

        nameof(IAuditable.UpdatedAt),

        nameof(IAuditable.UpdatedBy),

        nameof(ISoftDeletable.IsDeleted),

        "ConcurrencyStamp",

        "SecurityStamp",

        "NormalizedUserName",

        "NormalizedEmail",

        "AccessFailedCount",

        "LockoutEnd",

        "LastLoginAt",

        nameof(Teacher.ExpiryReminderSentAt)
    ];

    private static readonly HashSet<string> RedactedProperties = ["PasswordHash"];

    private List<PendingAudit> pending = [];

    private IDbContextTransaction? ownTransaction;

    private bool writingAudit;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (!writingAudit && eventData.Context is { } context)
        {
            Collect(context);

            if (pending.Count > 0 && context.Database.CurrentTransaction is null)
            {
                ownTransaction = context.Database.BeginTransaction();
            }
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (!writingAudit && eventData.Context is { } context)
        {
            Collect(context);

            if (pending.Count > 0 && context.Database.CurrentTransaction is null)
            {
                ownTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (!writingAudit && eventData.Context is { } context && pending.Count > 0)
        {
            AddLogs(context);

            writingAudit = true;

            try
            {
                context.SaveChanges();

                ownTransaction?.Commit();
            }
            finally
            {
                writingAudit = false;

                Reset();
            }
        }

        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (!writingAudit && eventData.Context is { } context && pending.Count > 0)
        {
            AddLogs(context);

            writingAudit = true;

            try
            {
                await context.SaveChangesAsync(cancellationToken);

                if (ownTransaction is not null)
                {
                    await ownTransaction.CommitAsync(cancellationToken);
                }
            }
            finally
            {
                writingAudit = false;

                Reset();
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (!writingAudit)
        {
            Reset();
        }

        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (!writingAudit)
        {
            Reset();
        }

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void Collect(DbContext context)
    {
        pending = [];

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog && entry.State is EntityState.Modified or EntityState.Deleted)
            {
                throw new InvalidOperationException("Audit log rows are append-only and can't be changed or deleted.");
            }

            if (!IsAudited(entry))
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    // The new values are read after the save, when generated keys and foreign keys are real.
                    pending.Add(new PendingAudit(entry, AuditActions.Create, null, null));

                    break;

                case EntityState.Deleted:
                    pending.Add(new PendingAudit(entry, AuditActions.Delete, Snapshot(entry, property => property.OriginalValue), null));

                    break;

                case EntityState.Modified when entry.Entity is ISoftDeletable { IsDeleted: true } && entry.Property(nameof(ISoftDeletable.IsDeleted)).IsModified:
                    pending.Add(new PendingAudit(entry, AuditActions.Delete, Snapshot(entry, property => property.OriginalValue), null));

                    break;

                case EntityState.Modified:
                    var before = new Dictionary<string, object?>();

                    var after = new Dictionary<string, object?>();

                    foreach (var property in entry.Properties)
                    {
                        if (!property.IsModified || IgnoredProperties.Contains(property.Metadata.Name) || Equals(property.OriginalValue, property.CurrentValue))
                        {
                            continue;
                        }

                        before[property.Metadata.Name] = Value(property.Metadata.Name, property.OriginalValue);

                        after[property.Metadata.Name] = Value(property.Metadata.Name, property.CurrentValue);
                    }

                    if (after.Count > 0)
                    {
                        pending.Add(new PendingAudit(entry, AuditActions.Update, before, after));
                    }

                    break;
            }
        }
    }

    private void AddLogs(DbContext context)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var logs = pending.Select(item => new AuditLog
        {
            TeacherId = TeacherOf(item.Entry),

            UserId = currentUser.UserId,

            UserName = currentUser.UserName,

            UserRole = currentUser.Role,

            Action = item.Action,

            EntityName = item.Entry.Metadata.ClrType.Name,

            EntityId = KeyOf(item.Entry),

            OldValues = Serialize(item.OldValues),

            NewValues = Serialize(item.Action == AuditActions.Create ? Snapshot(item.Entry, property => property.CurrentValue) : item.NewValues),

            IpAddress = currentUser.IpAddress,

            UserAgent = Truncate(currentUser.UserAgent, 512),

            CreatedAt = now
        });

        context.Set<AuditLog>().AddRange(logs);
    }

    private void Reset()
    {
        pending = [];

        ownTransaction?.Dispose();

        ownTransaction = null;
    }

    private static bool IsAudited(EntityEntry entry)
    {
        var type = entry.Metadata.ClrType;

        return entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
            && !SkippedTypes.Contains(type)
            && type.GetCustomAttribute<NotAuditedAttribute>() is null;
    }

    private static Dictionary<string, object?> Snapshot(EntityEntry entry, Func<PropertyEntry, object?> read)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            var name = property.Metadata.Name;

            var value = read(property);

            if (property.Metadata.IsPrimaryKey() && property.Metadata.ValueGenerated != ValueGenerated.Never || IgnoredProperties.Contains(name) || value is null)
            {
                continue;
            }

            values[name] = Value(name, value);
        }

        return values;
    }

    private static string? Serialize(Dictionary<string, object?>? values) => values is null || values.Count == 0 ? null : JsonSerializer.Serialize(values, JsonOptions);

    private static object? Value(string propertyName, object? value) => RedactedProperties.Contains(propertyName) ? "[redacted]" : value;

    private static string? KeyOf(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();

        if (key is null)
        {
            return null;
        }

        var id = string.Join(",", key.Properties.Select(property => entry.Property(property.Name).CurrentValue?.ToString()));

        return id.Length == 0 ? null : id;
    }

    private int? TeacherOf(EntityEntry entry)
    {
        if (entry.Entity is Teacher teacher)
        {
            return teacher.Id;
        }

        if (entry.Entity is ITenantEntity tenantEntity)
        {
            return tenantEntity.TeacherId;
        }

        if (entry.Entity is User user)
        {
            return user.TeacherId;
        }

        return currentUser.TeacherId;
    }

    private static string? Truncate(string? value, int length) => value is null || value.Length <= length ? value : value[..length];

    private sealed record PendingAudit(EntityEntry Entry, string Action, Dictionary<string, object?>? OldValues, Dictionary<string, object?>? NewValues);
}
