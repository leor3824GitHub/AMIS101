using FSH.Modules.Expendable.Domain.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Expendable.Data.Configurations;

public class SupplyRequestConfiguration : IEntityTypeConfiguration<SupplyRequest>
{
    public void Configure(EntityTypeBuilder<SupplyRequest> builder)
    {
        builder.ToTable($"{nameof(SupplyRequest)}s", ExpenableModuleConstants.SchemaName);

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.TenantId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.RequestNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.EmployeeId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.DepartmentId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<int>();

        builder.Property(p => p.BusinessJustification)
            .HasMaxLength(1000);

        builder.Property(p => p.RejectionReason)
            .HasMaxLength(500);

        builder.Property(p => p.ApprovedBy)
            .HasMaxLength(50);

        builder.Property(p => p.Version)
            .IsRowVersion();

        // Items (Owned Type)
        builder.OwnsMany(p => p.Items, ob =>
        {
            ob.ToJson("Items");
        });

        // Indexes
        builder.HasIndex(p => new { p.TenantId, p.RequestNumber })
            .IsUnique();

        builder.HasIndex(p => new { p.TenantId, p.Status });

        builder.HasIndex(p => new { p.TenantId, p.EmployeeId });

        builder.HasIndex(p => new { p.TenantId, p.DepartmentId });

        // BaseDbContext handles tenant scoping; keep explicit soft-delete filter.
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Soft Delete
        builder.Property(p => p.IsDeleted)
            .HasDefaultValue(false);
    }
}
