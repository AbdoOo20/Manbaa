using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Manbaa.Domain.Entities;

namespace Manbaa.Infrastructure.Persistence.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.Property(teacher => teacher.FullName).HasMaxLength(150).IsRequired();

        builder.Property(teacher => teacher.Subject).HasMaxLength(100).IsRequired();

        builder.Property(teacher => teacher.Phone).HasMaxLength(20).IsRequired();

        builder.Property(teacher => teacher.Email).HasMaxLength(256);

        builder.Property(teacher => teacher.Notes).HasMaxLength(1000);

        builder.Property(teacher => teacher.SuspensionReason).HasMaxLength(500);

        builder.Property(teacher => teacher.SearchText).HasMaxLength(800).IsRequired();

        builder.HasIndex(teacher => teacher.Phone);

        builder.HasIndex(teacher => new { teacher.Status, teacher.SubscriptionEndsAt });

        builder.HasOne(teacher => teacher.Plan).WithMany().HasForeignKey(teacher => teacher.PlanId).OnDelete(DeleteBehavior.Restrict);
    }
}
