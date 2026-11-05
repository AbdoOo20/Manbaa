using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Manbaa.Domain.Entities;

namespace Manbaa.Infrastructure.Persistence.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.Property(plan => plan.Name).HasMaxLength(100).IsRequired();

        builder.Property(plan => plan.Description).HasMaxLength(500);

        builder.Property(plan => plan.MonthlyPrice).HasPrecision(18, 2);

        builder.HasIndex(plan => plan.Name).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
