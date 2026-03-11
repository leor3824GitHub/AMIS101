using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using FSH.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FSH.Modules.MasterData.Data.Configurations;

public sealed class EmployeeProfileConfiguration : IEntityTypeConfiguration<EmployeeProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeProfile> builder)
    {
        builder.ToTable("EmployeeProfiles", MasterDataModuleConstants.SchemaName)
            .IsMultiTenant();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EmployeeNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.IdentityUserId).HasMaxLength(64);
        builder.Property(x => x.FirstName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.WorkEmail).HasMaxLength(256);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasOne(x => x.Office)
            .WithMany()
            .HasForeignKey(x => x.OfficeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DefaultUnitOfMeasure)
            .WithMany()
            .HasForeignKey(x => x.DefaultUnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.EmployeeNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.IdentityUserId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.OfficeId });
        builder.HasIndex(x => new { x.TenantId, x.DepartmentId });
        builder.HasIndex(x => new { x.TenantId, x.PositionId });

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter("SoftDelete", x => !x.IsDeleted);
    }
}

