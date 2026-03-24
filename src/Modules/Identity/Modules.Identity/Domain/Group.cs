using FSH.Framework.Core.Domain;

namespace FSH.Modules.Identity.Domain;

public class Group : ISoftDeletable
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsSystemGroup { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public string? ModifiedBy { get; private set; }

    // ISoftDeletable implementation
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation properties
    public virtual ICollection<GroupRole> GroupRoles { get; private set; } = [];
    public virtual ICollection<UserGroup> UserGroups { get; private set; } = [];

    private Group() { } // EF Core

    public static Group Create(string name, string? description = null, bool isDefault = false, bool isSystemGroup = false, string? createdBy = null)
    {
        return new Group
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsDefault = isDefault,
            IsSystemGroup = isSystemGroup,
<<<<<<< HEAD
            CreatedAt = DateTime.UtcNow,
=======
            CreatedOnUtc = TimeProvider.System.GetUtcNow(),
>>>>>>> d964bcda (fix(identity): align Group entity with IAuditableEntity and encapsulate soft-delete)
            CreatedBy = createdBy
        };
    }

    public void Update(string name, string? description, string? modifiedBy = null)
    {
        Name = name;
        Description = description;
<<<<<<< HEAD
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
=======
        LastModifiedOnUtc = TimeProvider.System.GetUtcNow();
        LastModifiedBy = modifiedBy;
>>>>>>> d964bcda (fix(identity): align Group entity with IAuditableEntity and encapsulate soft-delete)
    }

    public void SetAsDefault(bool isDefault, string? modifiedBy = null)
    {
        IsDefault = isDefault;
<<<<<<< HEAD
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
=======
        LastModifiedOnUtc = TimeProvider.System.GetUtcNow();
        LastModifiedBy = modifiedBy;
    }

    public void Delete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOnUtc = TimeProvider.System.GetUtcNow();
        DeletedBy = deletedBy;
>>>>>>> d964bcda (fix(identity): align Group entity with IAuditableEntity and encapsulate soft-delete)
    }
}
