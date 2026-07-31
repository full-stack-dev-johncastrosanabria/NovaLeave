using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLeave.Domain.Entities;

namespace NovaLeave.Infrastructure.Persistence.Configurations;

public sealed class AuditRecordConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    public void Configure(EntityTypeBuilder<AuditRecord> builder)
    {
        builder.ToTable("AuditRecord");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.ActorId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(record => record.ActorRole)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(record => record.Action)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(record => record.EntityType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(record => record.Result)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(record => record.Data)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(record => new { record.EntityType, record.EntityId })
            .HasDatabaseName("IX_AuditRecord_Entity");

        builder.HasIndex(record => record.TimestampUtc)
            .HasDatabaseName("IX_AuditRecord_TimestampUtc");
    }
}
