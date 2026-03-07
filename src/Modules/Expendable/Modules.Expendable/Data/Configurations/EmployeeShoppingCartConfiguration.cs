using FSH.Modules.Expendable.Domain.Cart;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Expendable.Data.Configurations;

public class EmployeeShoppingCartConfiguration : IEntityTypeConfiguration<EmployeeShoppingCart>
{
    public void Configure(EntityTypeBuilder<EmployeeShoppingCart> builder)
    {
        builder.ToTable($"{nameof(EmployeeShoppingCart)}s", ExpenableModuleConstants.SchemaName);

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.TenantId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.EmployeeId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<int>();

        builder.Property(p => p.Version)
            .IsRowVersion();

        // Items (Owned Type)
        builder.OwnsMany(p => p.Items, ob =>
        {
            ob.ToJson("Items");
            ob.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });

        // Indexes
        builder.HasIndex(p => new { p.TenantId, p.EmployeeId })
            .IsUnique(false);

        builder.HasIndex(p => new { p.TenantId, p.Status });

        // BaseDbContext handles tenant scoping; keep explicit soft-delete filter.
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Soft Delete
        builder.Property(p => p.IsDeleted)
            .HasDefaultValue(false);
    }
}
