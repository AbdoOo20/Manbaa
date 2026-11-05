using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Manbaa.Domain.Entities;

namespace Manbaa.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(notification => notification.Type).HasMaxLength(50).IsRequired();

        builder.Property(notification => notification.Title).HasMaxLength(200).IsRequired();

        builder.Property(notification => notification.Body).HasMaxLength(1000).IsRequired();

        builder.Property(notification => notification.Link).HasMaxLength(300);

        builder.HasIndex(notification => new { notification.UserId, notification.IsRead, notification.Id });
    }
}
