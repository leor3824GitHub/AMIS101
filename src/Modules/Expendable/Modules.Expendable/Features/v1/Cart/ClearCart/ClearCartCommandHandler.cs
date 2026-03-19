using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Contracts.v1.Cart;
using FSH.Modules.Expendable.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Cart.ClearCart;

public sealed class ClearCartCommandHandler : ICommandHandler<ClearCartCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public ClearCartCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(ClearCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.ShoppingCarts
            .FirstOrDefaultAsync(c => c.Id == command.CartId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Cart {command.CartId} not found.");

        CartAccessGuard.EnsureCanAccessCart(_currentUser, cart);

        cart.Clear();
        cart.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}
