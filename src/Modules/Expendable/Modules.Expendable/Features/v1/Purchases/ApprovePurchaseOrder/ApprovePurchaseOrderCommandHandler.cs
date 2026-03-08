using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Contracts.v1.Purchases;
using FSH.Modules.Expendable.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Purchases.ApprovePurchaseOrder;

public sealed class ApprovePurchaseOrderCommandHandler : ICommandHandler<ApprovePurchaseOrderCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public ApprovePurchaseOrderCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
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
