using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLeave.Domain.Entities;

namespace NovaLeave.Infrastructure.Persistence.Configurations;

public sealed class BalanceMovementConfiguration : IEntityTypeConfiguration<BalanceMovement>
{
    public void Configure(EntityTypeBuilder<BalanceMovement> builder)
    {
        builder.ToTable("BalanceMovement");

        builder.HasKey(movement => movement.Id);

        builder.Property(movement => movement.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(movement => movement.ActorId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(movement => new { movement.BalanceId, movement.EffectiveAtUtc })
            .HasDatabaseName("IX_BalanceMovement_BalanceId_EffectiveAt");

        builder.HasIndex(movement => movement.RequestId)
            .HasDatabaseName("IX_BalanceMovement_RequestId");

        builder.HasIndex(movement => new { movement.BalanceId, movement.AccrualPeriod, movement.Type })
            .HasDatabaseName("UX_BalanceMovement_Accrual")
            .IsUnique()
            .HasFilter("[AccrualPeriod] IS NOT NULL AND [Type] = 'Accrual'");

        builder.HasOne<VacationRequest>()
            .WithMany()
            .HasForeignKey(movement => movement.RequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
