using FSH.Modules.Expendable.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Expendable.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable($"{nameof(Product)}s", ExpenableModuleConstants.SchemaName);

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.TenantId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.SKU)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.UnitOfMeasure)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<int>();

        builder.Property(p => p.Version)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(p => new { p.TenantId, p.SKU })
            .IsUnique();

        builder.HasIndex(p => new { p.TenantId, p.Status });

        // BaseDbContext handles tenant scoping; keep explicit soft-delete filter.
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Soft Delete
        builder.Property(p => p.IsDeleted)
            .HasDefaultValue(false);
    }
}
