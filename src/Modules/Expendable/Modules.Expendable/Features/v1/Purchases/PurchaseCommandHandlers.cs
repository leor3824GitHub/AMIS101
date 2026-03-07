using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Contracts.v1.Purchases;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Purchases;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Purchases;

public sealed class CreatePurchaseOrderCommandHandler : ICommandHandler<CreatePurchaseOrderCommand, PurchaseDto>
{
    private readonly ExpenableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public CreatePurchaseOrderCommandHandler(ExpenableDbContext dbContext, ICurrentUser currentUser)
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
            command.ExpectedDeliveryDate);

        purchase.CreatedBy = _currentUser.GetUserId().ToString();

        _dbContext.Purchases.Add(purchase);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return purchase.ToPurchaseDto();
    }
}

public sealed class AddPurchaseLineItemCommandHandler : ICommandHandler<AddPurchaseLineItemCommand>
{
    private readonly ExpenableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public AddPurchaseLineItemCommandHandler(ExpenableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(AddPurchaseLineItemCommand command, CancellationToken cancellationToken)
    {
        var purchase = await _dbContext.Purchases
            .FirstOrDefaultAsync(p => p.Id == command.PurchaseId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Purchase {command.PurchaseId} not found.");

        purchase.AddLineItem(command.ProductId, command.Quantity, command.UnitPrice);
        purchase.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class RemovePurchaseLineItemCommandHandler : ICommandHandler<RemovePurchaseLineItemCommand>
{
    private readonly ExpenableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public RemovePurchaseLineItemCommandHandler(ExpenableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(RemovePurchaseLineItemCommand command, CancellationToken cancellationToken)
    {
        var purchase = await _dbContext.Purchases
            .FirstOrDefaultAsync(p => p.Id == command.PurchaseId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Purchase {command.PurchaseId} not found.");

        purchase.RemoveLineItem(command.ProductId);
        purchase.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class SubmitPurchaseOrderCommandHandler : ICommandHandler<SubmitPurchaseOrderCommand>
{
    private readonly ExpenableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public SubmitPurchaseOrderCommandHandler(ExpenableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(SubmitPurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        var purchase = await _dbContext.Purchases
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Purchase {command.Id} not found.");

        purchase.Submit();
        purchase.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class ApprovePurchaseOrderCommandHandler : ICommandHandler<ApprovePurchaseOrderCommand>
{
    private readonly ExpenableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public ApprovePurchaseOrderCommandHandler(ExpenableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(ApprovePurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        var purchase = await _dbContext.Purchases
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Purchase {command.Id} not found.");

        purchase.Approve();
        purchase.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class RecordPurchaseReceiptCommandHandler : ICommandHandler<RecordPurchaseReceiptCommand>
{
    private readonly ExpenableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public RecordPurchaseReceiptCommandHandler(ExpenableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(RecordPurchaseReceiptCommand command, CancellationToken cancellationToken)
    {
        var purchase = await _dbContext.Purchases
            .FirstOrDefaultAsync(p => p.Id == command.PurchaseId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Purchase {command.PurchaseId} not found.");

        purchase.RecordReceipt(command.ProductId, command.ReceivedQuantity, command.RejectedQuantity);
        purchase.ReceivingNotes = command.ReceivingNotes;
        purchase.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class CancelPurchaseOrderCommandHandler : ICommandHandler<CancelPurchaseOrderCommand>
{
    private readonly ExpenableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public CancelPurchaseOrderCommandHandler(ExpenableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(CancelPurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        var purchase = await _dbContext.Purchases
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Purchase {command.Id} not found.");

        purchase.Cancel();
        purchase.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

