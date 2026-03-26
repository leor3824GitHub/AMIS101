using FSH.Framework.Core.Domain;
using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Domain;

namespace Identity.Tests.Domain;

/// <summary>
/// Unit tests for the Group domain entity covering:
/// - IAuditableEntity properties set on creation
/// - Update() and SetAsDefault() set LastModifiedOnUtc/LastModifiedBy
/// - Delete() encapsulates soft-delete (ISoftDeletable)
/// - GroupDto.CreatedAt is DateTimeOffset
/// </summary>
public sealed class GroupTests
{
    #region Create

    [Fact]
    public void Create_Should_SetIAuditableEntityProperties()
    {
        // Arrange
        var before = TimeProvider.System.GetUtcNow();

        // Act
        var group = Group.Create("Admins", "Admin group", createdBy: "user-1");

        var after = TimeProvider.System.GetUtcNow();

        // Assert
        group.CreatedBy.ShouldBe("user-1");
        group.CreatedOnUtc.ShouldBeGreaterThanOrEqualTo(before);
        group.CreatedOnUtc.ShouldBeLessThanOrEqualTo(after);
        group.LastModifiedOnUtc.ShouldBeNull();
        group.LastModifiedBy.ShouldBeNull();
    }

    [Fact]
    public void Create_Should_ImplementIAuditableEntity()
    {
        // Act
        var group = Group.Create("Admins");

        // Assert
        group.ShouldBeAssignableTo<IAuditableEntity>();
    }

    [Fact]
    public void Create_Should_ImplementISoftDeletable()
    {
        // Act
        var group = Group.Create("Admins");

        // Assert
        group.ShouldBeAssignableTo<ISoftDeletable>();
    }

    [Fact]
    public void Create_Should_NotBeDeleted()
    {
        // Act
        var group = Group.Create("Admins");

        // Assert
        group.IsDeleted.ShouldBeFalse();
        group.DeletedOnUtc.ShouldBeNull();
        group.DeletedBy.ShouldBeNull();
    }

    #endregion

    #region Update

    [Fact]
    public void Update_Should_SetLastModifiedProperties()
    {
        // Arrange
        var group = Group.Create("OldName", createdBy: "creator");
        var before = TimeProvider.System.GetUtcNow();

        // Act
        group.Update("NewName", "New description", modifiedBy: "editor-1");

        var after = TimeProvider.System.GetUtcNow();

        // Assert
        group.Name.ShouldBe("NewName");
        group.Description.ShouldBe("New description");
        group.LastModifiedBy.ShouldBe("editor-1");
        group.LastModifiedOnUtc.ShouldNotBeNull();
        group.LastModifiedOnUtc!.Value.ShouldBeGreaterThanOrEqualTo(before);
        group.LastModifiedOnUtc.Value.ShouldBeLessThanOrEqualTo(after);
    }

    #endregion

    #region SetAsDefault

    [Fact]
    public void SetAsDefault_Should_SetLastModifiedProperties()
    {
        // Arrange
        var group = Group.Create("MyGroup");
        var before = TimeProvider.System.GetUtcNow();

        // Act
        group.SetAsDefault(true, modifiedBy: "admin");

        var after = TimeProvider.System.GetUtcNow();

        // Assert
        group.IsDefault.ShouldBeTrue();
        group.LastModifiedBy.ShouldBe("admin");
        group.LastModifiedOnUtc.ShouldNotBeNull();
        group.LastModifiedOnUtc!.Value.ShouldBeGreaterThanOrEqualTo(before);
        group.LastModifiedOnUtc.Value.ShouldBeLessThanOrEqualTo(after);
    }

    #endregion

    #region Delete

    [Fact]
    public void Delete_Should_MarkGroupAsDeleted()
    {
        // Arrange
        var group = Group.Create("Admins");
        var before = TimeProvider.System.GetUtcNow();

        // Act
        group.Delete("deleter-1");

        var after = TimeProvider.System.GetUtcNow();

        // Assert
        group.IsDeleted.ShouldBeTrue();
        group.DeletedBy.ShouldBe("deleter-1");
        group.DeletedOnUtc.ShouldNotBeNull();
        group.DeletedOnUtc!.Value.ShouldBeGreaterThanOrEqualTo(before);
        group.DeletedOnUtc.Value.ShouldBeLessThanOrEqualTo(after);
    }

    [Fact]
    public void Delete_Should_WorkWithNullDeletedBy()
    {
        // Arrange
        var group = Group.Create("Admins");

        // Act
        group.Delete();

        // Assert
        group.IsDeleted.ShouldBeTrue();
        group.DeletedBy.ShouldBeNull();
        group.DeletedOnUtc.ShouldNotBeNull();
    }

    [Fact]
    public void Delete_Should_NotAffectCreatedOnUtc()
    {
        // Arrange
        var group = Group.Create("Admins", createdBy: "creator");
        var createdOnUtc = group.CreatedOnUtc;

        // Act
        group.Delete("deleter");

        // Assert
        group.CreatedOnUtc.ShouldBe(createdOnUtc);
    }

    #endregion

    #region GroupDto

    [Fact]
    public void GroupDto_CreatedAt_ShouldBe_DateTimeOffset()
    {
        // Arrange
        var dto = new GroupDto { CreatedAt = DateTimeOffset.UtcNow };

        // Assert - verify the property is of DateTimeOffset type
        dto.CreatedAt.ShouldBeOfType<DateTimeOffset>();
    }

    [Fact]
    public void GroupDto_CreatedAt_ShouldPreserve_ValueFromGroup()
    {
        // Arrange
        var group = Group.Create("TestGroup", createdBy: "user");

        // Act
        var dto = new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            CreatedAt = group.CreatedOnUtc
        };

        // Assert
        dto.CreatedAt.ShouldBe(group.CreatedOnUtc);
    }

    #endregion
}
