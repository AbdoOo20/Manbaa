using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Manbaa.Domain.Entities;

namespace Manbaa.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <summary>Blocks UPDATE and DELETE on the table at the database level. Created in the InitialCreate migration.</summary>
    public const string ReadOnlyTrigger = "TR_AuditLogs_ReadOnly";

    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable(table => table.HasTrigger(ReadOnlyTrigger));

        builder.Property(log => log.UserName).HasMaxLength(150);

        builder.Property(log => log.UserRole).HasMaxLength(50);

        builder.Property(log => log.Action).HasMaxLength(50).IsRequired();

        builder.Property(log => log.EntityName).HasMaxLength(100).IsRequired();

        builder.Property(log => log.EntityId).HasMaxLength(100);

        builder.Property(log => log.IpAddress).HasMaxLength(64);

        builder.Property(log => log.UserAgent).HasMaxLength(512);

        builder.HasIndex(log => new { log.TeacherId, log.Id });

        builder.HasIndex(log => new { log.EntityName, log.EntityId });

        builder.HasIndex(log => new { log.UserId, log.Id });

        builder.HasIndex(log => log.CreatedAt);
    }
}
