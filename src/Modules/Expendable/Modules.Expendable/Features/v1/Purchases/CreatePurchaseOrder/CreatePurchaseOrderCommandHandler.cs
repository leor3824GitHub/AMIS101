using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Contracts.v1.Purchases;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Purchases;
using Mediator;

namespace FSH.Modules.Expendable.Features.v1.Purchases.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderCommandHandler : ICommandHandler<CreatePurchaseOrderCommand, PurchaseDto>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public CreatePurchaseOrderCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<PurchaseDto> Handle(CreatePurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        // Generate PO number (simplified - use your own logic)
        var poNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}";

        var purchase = Purchase.Create(
            _currentUser.GetTenant() ?? throw new InvalidOperationException("Tenant ID required"),
            poNumber,
            command.SupplierId,
            command.SupplierName,
            command.WarehouseLocationId,
            command.WarehouseLocationName,
            command.ExpectedDeliveryDate);

        purchase.CreatedBy = _currentUser.GetUserId().ToString();

        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return purchase.ToPurchaseDto();
    }
}
