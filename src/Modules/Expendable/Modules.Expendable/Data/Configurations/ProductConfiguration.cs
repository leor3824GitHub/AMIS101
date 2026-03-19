using FSH.Modules.Expendable.Domain.Products;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.Expendable.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable($"{nameof(Product)}s", ExpendableModuleConstants.SchemaName)
            .IsMultiTenant();

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
            .IsConcurrencyToken();

        builder.OwnsMany(p => p.Images, ob =>
        {
            ob.ToJson("Images");
            ob.Property(x => x.Url)
                .HasMaxLength(2048)
                .IsRequired();
        });

        // Indexes
        builder.HasIndex(p => new { p.TenantId, p.SKU })
            .IsUnique();

        builder.HasIndex(p => new { p.TenantId, p.Status });

        // Soft Delete
        builder.Property(p => p.IsDeleted)
            .HasDefaultValue(false);

        builder.HasQueryFilter("SoftDelete", p => !p.IsDeleted);
    }
}

