using FSH.Framework.Core.Context;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.Expendable.Contracts.v1.Cart;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Cart;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Cart.GetCart;

public sealed class GetCartQueryHandler : IQueryHandler<GetCartQuery, EmployeeShoppingCartDto?>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public GetCartQueryHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<EmployeeShoppingCartDto?> Handle(GetCartQuery query, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.ShoppingCarts
            .FirstOrDefaultAsync(c => c.Id == query.CartId, cancellationToken)
            .ConfigureAwait(false);

        if (cart is null)
        {
            return null;
        }

        CartAccessGuard.EnsureCanAccessCart(_currentUser, cart);
        return cart.ToEmployeeShoppingCartDto();
    }
}
