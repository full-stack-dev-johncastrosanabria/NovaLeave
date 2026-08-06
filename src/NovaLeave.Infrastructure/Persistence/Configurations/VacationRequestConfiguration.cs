using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaLeave.Domain.Entities;
using NovaLeave.Infrastructure.Identity;

namespace NovaLeave.Infrastructure.Persistence.Configurations;

public sealed class VacationRequestConfiguration : IEntityTypeConfiguration<VacationRequest>
{
    public void Configure(EntityTypeBuilder<VacationRequest> builder)
    {
        builder.ToTable("VacationRequest");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.OwnerId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(request => request.LeaveType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(request => request.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(request => request.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(request => request.RejectionReason)
            .HasMaxLength(500);

        builder.Property(request => request.RowVersion)
            .IsConcurrencyToken()
            .IsRequired()
            .ValueGeneratedOnAddOrUpdate()
            .HasColumnType("rowversion");

        builder.HasIndex(request => new { request.OwnerId, request.Status })
            .HasDatabaseName("IX_VacationRequest_OwnerId_Status");

        builder.HasIndex(request => new { request.StartDate, request.EndDate })
            .HasDatabaseName("IX_VacationRequest_StartDate_EndDate");

        builder.HasIndex(request => new { request.OwnerId, request.StartDate, request.EndDate, request.Status })
            .HasDatabaseName("UX_VacationRequest_Owner_DateRange_Status")
            .IsUnique()
            .HasFilter("[Status] IN ('Pending', 'Approved')");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(request => request.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
