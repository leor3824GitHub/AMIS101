using FSH.Framework.Caching;
using FSH.Modules.Expendable.Contracts.v1.Warehouse;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Inventory;
using FSH.Modules.Expendable.Domain.Purchases;
using FSH.Modules.Expendable.Domain.Warehouse;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Warehouse;

/// <summary>Handle inspection of received purchase items</summary>
public sealed class RecordInspectionCommandHandler : ICommandHandler<RecordInspectionCommand, RecordInspectionResponse>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICacheService _cache;

    public RecordInspectionCommandHandler(ExpendableDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async ValueTask<RecordInspectionResponse> Handle(RecordInspectionCommand command, CancellationToken cancellationToken)
    {
        // Get purchase and line item
        var purchase = await _dbContext.Purchases
            .FirstOrDefaultAsync(p => p.Id == command.PurchaseId, cancellationToken)
            ?? throw new InvalidOperationException($"Purchase {command.PurchaseId} not found");

        var lineItem = purchase.LineItems.FirstOrDefault(li => li.ProductId == command.ProductId)
            ?? throw new InvalidOperationException($"Line item not found in purchase");

        if (lineItem.QuantityInspection < command.QuantityAccepted + command.QuantityRejected)
            throw new InvalidOperationException("Inspection quantities exceed pending amount");

        // Create inspection record
        var inspection = PurchaseInspection.Create(
            purchase.TenantId,
            command.PurchaseId,
            command.ProductId,
            lineItem.QuantityInspection,
            Guid.NewGuid(), // Inspector ID - should come from ICurrentUser
            purchase.WarehouseLocationId
        );

        // Record inspection result
        var defects = command.Defects == null ? null : new System.Collections.ObjectModel.Collection<InspectionDefect>(
            command.Defects.Select(d => new InspectionDefect
            {
                UnitNumber = d.UnitNumber,
                DefectDescription = d.Description ?? string.Empty,
                Severity = d.Severity
            }).ToList()
        );

        if (command.QuantityAccepted + command.QuantityRejected != lineItem.QuantityInspection)
            throw new InvalidOperationException(
                $"Accepted + Rejected must equal total received ({lineItem.QuantityInspection})");

        if (command.QuantityAccepted > 0 && command.QuantityRejected > 0)
        {
            inspection.MarkPartialAcceptance(
                command.QuantityAccepted,
                command.QuantityRejected,
                command.RejectionReason,
                command.Notes,
                defects
            );
        }
        else if (command.QuantityAccepted == lineItem.QuantityInspection)
        {
            inspection.MarkFullyAccepted(command.Notes);
        }
        else if (command.QuantityRejected == lineItem.QuantityInspection)
        {
            inspection.MarkFullyRejected(command.RejectionReason, command.Notes);
        }

        _dbContext.PurchaseInspections.Add(inspection);

        // Update purchase with inspection results
        purchase.CompleteInspection(command.ProductId, command.QuantityAccepted, command.QuantityRejected);

        // If accepted items, add to ProductInventory
        if (command.QuantityAccepted > 0)
        {
            var productInventory = await _dbContext.ProductInventories
                .FirstOrDefaultAsync(pi =>
                    pi.ProductId == command.ProductId &&
                    pi.WarehouseLocationId == purchase.WarehouseLocationId,
                    cancellationToken);

            if (productInventory == null)
            {
                // Create new ProductInventory for this warehouse
                productInventory = ProductInventory.Create(
                    purchase.TenantId,
                    command.ProductId,
                    lineItem.ProductCode,
                    lineItem.ProductName,
                    purchase.WarehouseLocationId,
                    purchase.WarehouseLocationName
                );
                _dbContext.ProductInventories.Add(productInventory);
            }

            productInventory.ReceiveFromPurchase(
                command.PurchaseId,
                command.ProductId,
                command.QuantityAccepted,
                lineItem.UnitPrice
            );
        }

        // If rejected items, create RejectedInventory record
        if (command.QuantityRejected > 0)
        {
            var rejectedInventory = RejectedInventory.Create(
                command.PurchaseId,
                command.ProductId,
                inspection.Id,
                lineItem.ProductCode,
                lineItem.ProductName,
                purchase.WarehouseLocationId,
                purchase.WarehouseLocationName,
                command.QuantityRejected,
                lineItem.UnitPrice,
                command.RejectionReason,
                purchase.TenantId,
                command.Notes
            );
            _dbContext.RejectedInventories.Add(rejectedInventory);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveItemAsync($"purchase:{command.PurchaseId}", cancellationToken);
        await _cache.RemoveItemAsync($"inventory:warehouse:{purchase.WarehouseLocationId}", cancellationToken);

        return new RecordInspectionResponse(
            inspection.Id,
            inspection.Status.ToString(),
            inspection.QuantityAccepted,
            inspection.QuantityRejected
        );
    }
}

/// <summary>Reserve product inventory for supply request</summary>
public sealed class ReserveProductInventoryCommandHandler : ICommandHandler<ReserveProductInventoryCommand, ReserveProductInventoryResponse>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICacheService _cache;

    public ReserveProductInventoryCommandHandler(ExpendableDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async ValueTask<ReserveProductInventoryResponse> Handle(ReserveProductInventoryCommand command, CancellationToken cancellationToken)
    {
        var inventory = await _dbContext.ProductInventories
            .FirstOrDefaultAsync(pi => pi.Id == command.ProductInventoryId, cancellationToken)
            ?? throw new InvalidOperationException($"ProductInventory {command.ProductInventoryId} not found");

        inventory.ReserveForAllocation(command.QuantityToReserve);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cache.RemoveItemAsync($"inventory:{command.ProductInventoryId}", cancellationToken);

        return new ReserveProductInventoryResponse(
            inventory.Id,
            inventory.QuantityAvailable,
            inventory.QuantityReserved
        );
    }
}

/// <summary>Cancel reservation on rejected supply request</summary>
public sealed class CancelProductInventoryReservationCommandHandler : ICommandHandler<CancelProductInventoryReservationCommand, CancelProductInventoryReservationResponse>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICacheService _cache;

    public CancelProductInventoryReservationCommandHandler(ExpendableDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async ValueTask<CancelProductInventoryReservationResponse> Handle(CancelProductInventoryReservationCommand command, CancellationToken cancellationToken)
    {
        var inventory = await _dbContext.ProductInventories
            .FirstOrDefaultAsync(pi => pi.Id == command.ProductInventoryId, cancellationToken)
            ?? throw new InvalidOperationException($"ProductInventory {command.ProductInventoryId} not found");

        inventory.CancelReservation(command.QuantityToRelease);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cache.RemoveItemAsync($"inventory:{command.ProductInventoryId}", cancellationToken);

        return new CancelProductInventoryReservationResponse(
            inventory.Id,
            inventory.QuantityAvailable,
            inventory.QuantityReserved
        );
    }
}

/// <summary>Issue reserved inventory to employee</summary>
public sealed class IssueFromProductInventoryCommandHandler : ICommandHandler<IssueFromProductInventoryCommand, IssueFromProductInventoryResponse>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICacheService _cache;

    public IssueFromProductInventoryCommandHandler(ExpendableDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async ValueTask<IssueFromProductInventoryResponse> Handle(IssueFromProductInventoryCommand command, CancellationToken cancellationToken)
    {
        var inventory = await _dbContext.ProductInventories
            .FirstOrDefaultAsync(pi => pi.Id == command.ProductInventoryId, cancellationToken)
            ?? throw new InvalidOperationException($"ProductInventory {command.ProductInventoryId} not found");

        var issuedDetails = inventory.IssueFromBatches(command.QuantityToIssue);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cache.RemoveItemAsync($"inventory:{command.ProductInventoryId}", cancellationToken);

        var response = new IssueFromProductInventoryResponse(
            inventory.Id,
            command.QuantityToIssue,
            issuedDetails.Sum(d => d.TotalValue),
            issuedDetails.Select(d => new IssuedBatchDetailDto(
                d.PurchaseId,
                d.ProductId,
                d.QuantityIssued,
                d.UnitPrice,
                d.TotalValue
            )).ToList()
        );

        return response;
    }
}

/// <summary>Mark rejected inventory as returned to supplier</summary>
public sealed class MarkRejectedInventoryReturnedCommandHandler : ICommandHandler<MarkRejectedInventoryReturnedCommand, MarkRejectedInventoryReturnedResponse>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICacheService _cache;

    public MarkRejectedInventoryReturnedCommandHandler(ExpendableDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async ValueTask<MarkRejectedInventoryReturnedResponse> Handle(MarkRejectedInventoryReturnedCommand command, CancellationToken cancellationToken)
    {
        var rejected = await _dbContext.RejectedInventories
            .FirstOrDefaultAsync(ri => ri.Id == command.RejectedInventoryId, cancellationToken)
            ?? throw new InvalidOperationException($"Rejected inventory {command.RejectedInventoryId} not found");

        rejected.MarkAsReturned(command.QuantityReturned, command.Notes);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cache.RemoveItemAsync($"rejected-inventory:{command.RejectedInventoryId}", cancellationToken);

        return new MarkRejectedInventoryReturnedResponse(rejected.Id, rejected.Status.ToString());
    }
}

/// <summary>Mark rejected inventory as disposed</summary>
public sealed class MarkRejectedInventoryDisposedCommandHandler : ICommandHandler<MarkRejectedInventoryDisposedCommand, MarkRejectedInventoryDisposedResponse>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICacheService _cache;

    public MarkRejectedInventoryDisposedCommandHandler(ExpendableDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async ValueTask<MarkRejectedInventoryDisposedResponse> Handle(MarkRejectedInventoryDisposedCommand command, CancellationToken cancellationToken)
    {
        var rejected = await _dbContext.RejectedInventories
            .FirstOrDefaultAsync(ri => ri.Id == command.RejectedInventoryId, cancellationToken)
            ?? throw new InvalidOperationException($"Rejected inventory {command.RejectedInventoryId} not found");

        rejected.MarkAsDisposed(command.DisposalMethod, command.Notes);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cache.RemoveItemAsync($"rejected-inventory:{command.RejectedInventoryId}", cancellationToken);

        return new MarkRejectedInventoryDisposedResponse(rejected.Id, rejected.Status.ToString());
    }
}


