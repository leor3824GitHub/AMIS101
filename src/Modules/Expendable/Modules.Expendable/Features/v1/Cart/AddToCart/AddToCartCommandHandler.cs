using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Contracts.v1.Cart;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Products;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Cart.AddToCart;

public sealed class AddToCartCommandHandler : ICommandHandler<AddToCartCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public AddToCartCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(AddToCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.ShoppingCarts
            .FirstOrDefaultAsync(c => c.Id == command.CartId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Cart {command.CartId} not found.");

        CartAccessGuard.EnsureCanAccessCart(_currentUser, cart);

        var product = await _dbContext.Products
            .FirstOrDefaultAsync(
                p => p.Id == command.ProductId && !p.IsDeleted && p.Status == ProductStatus.Active,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Product {command.ProductId} is unavailable.");

        // Never trust client-sent pricing for cart totals; always use the current catalog price.
        cart.AddItem(command.ProductId, command.Quantity, product.UnitPrice);
        cart.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}
