using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Manbaa.Domain.Entities;

namespace Manbaa.Infrastructure.Persistence.Configurations;

public class TeacherSubscriptionConfiguration : IEntityTypeConfiguration<TeacherSubscription>
{
    public void Configure(EntityTypeBuilder<TeacherSubscription> builder)
    {
        builder.Property(subscription => subscription.Amount).HasPrecision(18, 2);

        builder.Property(subscription => subscription.Notes).HasMaxLength(500);

        builder.Property(subscription => subscription.CancelReason).HasMaxLength(500);

        builder.Ignore(subscription => subscription.ReceiptNumber);

        builder.HasIndex(subscription => new { subscription.TeacherId, subscription.IsCancelled });

        builder.HasIndex(subscription => subscription.CreatedAt);

        builder.HasOne(subscription => subscription.Teacher).WithMany(teacher => teacher.Subscriptions).HasForeignKey(subscription => subscription.TeacherId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(subscription => subscription.Plan).WithMany().HasForeignKey(subscription => subscription.PlanId).OnDelete(DeleteBehavior.Restrict);
    }
}
