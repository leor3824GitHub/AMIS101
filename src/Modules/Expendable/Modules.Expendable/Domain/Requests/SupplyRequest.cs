using FSH.Framework.Core.Domain;

namespace FSH.Modules.Expendable.Domain.Requests;

/// <summary>Supply request status enumeration</summary>
public enum SupplyRequestStatus
{
    None = 0,
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    Fulfilled = 5,
    Cancelled = 6
}

/// <summary>Supply request item value object</summary>
public class SupplyRequestItem
{
    public Guid ProductId { get; set; }
    public int RequestedQuantity { get; set; }
    public int ApprovedQuantity { get; set; }
    public int FulfilledQuantity { get; set; }
    public string? Notes { get; set; }

    public SupplyRequestItem(Guid productId, int requestedQuantity, string? notes = null)
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        ApprovedQuantity = 0;
        FulfilledQuantity = 0;
        Notes = notes;
    }
}

public class SupplyRequest : AggregateRoot<Guid>, IHasTenant, IAuditableEntity, ISoftDeletable
{
    public string TenantId { get; private set; } = default!;
    public string RequestNumber { get; private set; } = default!;
    public string EmployeeId { get; private set; } = default!;
    public string DepartmentId { get; set; } = default!;
    public DateTimeOffset RequestDate { get; private set; }
    public DateTimeOffset? NeededByDate { get; set; }
    public SupplyRequestStatus Status { get; set; } = SupplyRequestStatus.Draft;
    public string? BusinessJustification { get; set; }
    public string? RejectionReason { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedOnUtc { get; set; }
    public byte[] Version { get; set; } = default!;

    private readonly List<SupplyRequestItem> _items = [];
    public IReadOnlyCollection<SupplyRequestItem> Items => _items.AsReadOnly();

    // IAuditableEntity
    public DateTimeOffset CreatedOnUtc { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedOnUtc { get; set; }
    public string? LastModifiedBy { get; set; }

    // ISoftDeletable
    public DateTimeOffset? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>Factory method to create a new supply request</summary>
    public static SupplyRequest Create(string tenantId, string requestNumber, string employeeId,
        string departmentId, string? businessJustification = null, DateTimeOffset? neededBy = null)
    {
        return new SupplyRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequestNumber = requestNumber,
            EmployeeId = employeeId,
            DepartmentId = departmentId,
            RequestDate = DateTimeOffset.UtcNow,
            NeededByDate = neededBy,
            BusinessJustification = businessJustification,
            Status = SupplyRequestStatus.Draft,
            CreatedOnUtc = DateTimeOffset.UtcNow
        };
    }

    /// <summary>Add a supply item to the request</summary>
    public void AddItem(Guid productId, int quantity, string? notes = null)
    {
        if (Status != SupplyRequestStatus.Draft)
            throw new InvalidOperationException("Cannot add items to submitted requests.");

        var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.RequestedQuantity += quantity;
        }
        else
        {
            _items.Add(new SupplyRequestItem(productId, quantity, notes));
        }

        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Remove an item from the request</summary>
    public void RemoveItem(Guid productId)
    {
        if (Status != SupplyRequestStatus.Draft)
            throw new InvalidOperationException("Cannot remove items from submitted requests.");

        _items.RemoveAll(x => x.ProductId == productId);
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Submit the supply request</summary>
    public void Submit()
    {
        if (Status != SupplyRequestStatus.Draft)
            throw new InvalidOperationException("Only draft requests can be submitted.");

        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot submit request without items.");

        Status = SupplyRequestStatus.Submitted;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Approve the supply request</summary>
    public void Approve(string approvedBy, Dictionary<Guid, int> approvedQuantities)
    {
        if (Status != SupplyRequestStatus.Submitted)
            throw new InvalidOperationException("Only submitted requests can be approved.");

        foreach (var item in _items)
        {
            if (approvedQuantities.TryGetValue(item.ProductId, out var approvedQty))
            {
                if (approvedQty < 0 || approvedQty > item.RequestedQuantity)
                    throw new InvalidOperationException($"Approved quantity for product {item.ProductId} is invalid.");

                item.ApprovedQuantity = approvedQty;
            }
        }

        Status = SupplyRequestStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Reject the supply request</summary>
    public void Reject(string? reason = null)
    {
        if (Status != SupplyRequestStatus.Submitted)
            throw new InvalidOperationException("Only submitted requests can be rejected.");

        Status = SupplyRequestStatus.Rejected;
        RejectionReason = reason;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Mark as fulfilled</summary>
    public void MarkFulfilled()
    {
        if (Status != SupplyRequestStatus.Approved)
            throw new InvalidOperationException("Only approved requests can be marked as fulfilled.");

        Status = SupplyRequestStatus.Fulfilled;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Cancel the request</summary>
    public void Cancel()
    {
        if (Status == SupplyRequestStatus.Fulfilled || Status == SupplyRequestStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel fulfilled or already cancelled requests.");

        Status = SupplyRequestStatus.Cancelled;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Soft delete the request</summary>
    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }
}

