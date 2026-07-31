using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLeave.Domain.Entities;
using NovaLeave.Infrastructure.Identity;

namespace NovaLeave.Infrastructure.Persistence.Configurations;

public sealed class VacationBalanceConfiguration : IEntityTypeConfiguration<VacationBalance>
{
    public void Configure(EntityTypeBuilder<VacationBalance> builder)
    {
        builder.ToTable("VacationBalance");

        builder.HasKey(balance => balance.Id);

        builder.Property(balance => balance.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(balance => balance.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken(false);

        builder.Ignore(balance => balance.AvailableDays);

        builder.HasIndex(balance => balance.UserId)
            .HasDatabaseName("UX_VacationBalance_UserId")
            .IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<VacationBalance>(balance => balance.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(balance => balance.Movements)
            .WithOne()
            .HasForeignKey(movement => movement.BalanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(balance => balance.Movements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
