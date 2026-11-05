using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Manbaa.Domain.Entities;

namespace Manbaa.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.FullName).HasMaxLength(150).IsRequired();

        builder.HasIndex(user => new { user.TeacherId, user.PhoneNumber });

        builder.HasOne(user => user.Teacher).WithMany().HasForeignKey(user => user.TeacherId).OnDelete(DeleteBehavior.Restrict);
    }
}
