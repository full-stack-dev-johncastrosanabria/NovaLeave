using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLeave.Infrastructure.Identity;

namespace NovaLeave.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.IsActive)
            .IsRequired();

        builder.Property(user => user.CanResolveRequests)
            .IsRequired();

        builder.Property(user => user.EmploymentStartDate)
            .IsRequired();
    }
}
