using FSH.Framework.Core.Domain;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Domain;

namespace Identity.Tests.Domain;

/// <summary>
/// Unit tests for the Group domain entity.
/// Covers IAuditableEntity implementation, soft-delete encapsulation,
/// and GroupDto mapping of DateTimeOffset.CreatedAt.
/// </summary>
public sealed class GroupTests
{
    #region Create

    [Fact]
    public void Create_Sets_Id()
    {
        var group = Group.Create("Engineers");

        group.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Create_Sets_Name()
    {
        var group = Group.Create("Engineers");

        group.Name.ShouldBe("Engineers");
    }

    [Fact]
    public void Create_Sets_CreatedOnUtc_To_UtcNow()
    {
        var before = TimeProvider.System.GetUtcNow();
        var group = Group.Create("Engineers");
        var after = TimeProvider.System.GetUtcNow();

        group.CreatedOnUtc.ShouldBeInRange(before, after);
    }

    [Fact]
    public void Create_Sets_CreatedBy()
    {
        var group = Group.Create("Engineers", createdBy: "user-123");

        group.CreatedBy.ShouldBe("user-123");
    }

    [Fact]
    public void Create_Implements_IAuditableEntity()
    {
        var group = Group.Create("Engineers");

        group.ShouldBeAssignableTo<IAuditableEntity>();
    }

    [Fact]
    public void Create_LastModifiedOnUtc_Is_Null_Initially()
    {
        var group = Group.Create("Engineers");

        group.LastModifiedOnUtc.ShouldBeNull();
    }

    [Fact]
    public void Create_LastModifiedBy_Is_Null_Initially()
    {
        var group = Group.Create("Engineers");

        group.LastModifiedBy.ShouldBeNull();
    }

    [Fact]
    public void Create_IsDeleted_Is_False()
    {
        var group = Group.Create("Engineers");

        group.IsDeleted.ShouldBeFalse();
    }

    #endregion

    #region Update

    [Fact]
    public void Update_Sets_LastModifiedOnUtc()
    {
        var group = Group.Create("Engineers");
        var before = TimeProvider.System.GetUtcNow();
        group.Update("Developers", "Dev team", "modifier-1");
        var after = TimeProvider.System.GetUtcNow();

        group.LastModifiedOnUtc.ShouldNotBeNull();
        group.LastModifiedOnUtc!.Value.ShouldBeInRange(before, after);
    }

    [Fact]
    public void Update_Sets_LastModifiedBy()
    {
        var group = Group.Create("Engineers");
        group.Update("Developers", null, "modifier-1");

        group.LastModifiedBy.ShouldBe("modifier-1");
    }

    [Fact]
    public void Update_Changes_Name_And_Description()
    {
        var group = Group.Create("Engineers", "Original desc");
        group.Update("Developers", "New desc");

        group.Name.ShouldBe("Developers");
        group.Description.ShouldBe("New desc");
    }

    #endregion

    #region Delete (soft-delete encapsulation)

    [Fact]
    public void Delete_Sets_IsDeleted_True()
    {
        var group = Group.Create("Engineers");
        group.Delete();

        group.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void Delete_Sets_DeletedOnUtc()
    {
        var group = Group.Create("Engineers");
        var before = TimeProvider.System.GetUtcNow();
        group.Delete();
        var after = TimeProvider.System.GetUtcNow();

        group.DeletedOnUtc.ShouldNotBeNull();
        group.DeletedOnUtc!.Value.ShouldBeInRange(before, after);
    }

    [Fact]
    public void Delete_Sets_DeletedBy()
    {
        var group = Group.Create("Engineers");
        group.Delete("admin-user");

        group.DeletedBy.ShouldBe("admin-user");
    }

    [Fact]
    public void Delete_With_Null_DeletedBy_Is_Allowed()
    {
        var group = Group.Create("Engineers");
        group.Delete(null);

        group.IsDeleted.ShouldBeTrue();
        group.DeletedBy.ShouldBeNull();
    }

    [Fact]
    public void Delete_IsDeleted_Has_Private_Setter()
    {
        // Verify soft-delete encapsulation: IsDeleted cannot be set externally
        var prop = typeof(Group).GetProperty(nameof(Group.IsDeleted));
        prop.ShouldNotBeNull();
        var setter = prop!.GetSetMethod(nonPublic: true);
        setter.ShouldNotBeNull();
        setter!.IsPrivate.ShouldBeTrue();
    }

    #endregion

    #region GroupDto mapping

    [Fact]
    public void GroupDto_CreatedAt_Is_DateTimeOffset()
    {
        var prop = typeof(GroupDto).GetProperty(nameof(GroupDto.CreatedAt));

        prop.ShouldNotBeNull();
        prop!.PropertyType.ShouldBe(typeof(DateTimeOffset));
    }

    [Fact]
    public void GroupDto_CreatedAt_Preserves_Value_From_CreatedOnUtc()
    {
        var group = Group.Create("Engineers", createdBy: "creator");

        var dto = new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            IsDefault = group.IsDefault,
            IsSystemGroup = group.IsSystemGroup,
            MemberCount = 0,
            CreatedAt = group.CreatedOnUtc
        };

        dto.CreatedAt.ShouldBe(group.CreatedOnUtc);
        dto.CreatedAt.Offset.ShouldBe(TimeSpan.Zero); // UTC
    }

    #endregion
}
