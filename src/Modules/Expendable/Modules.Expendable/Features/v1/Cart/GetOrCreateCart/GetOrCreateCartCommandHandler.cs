using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Contracts.v1.Cart;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Cart;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Cart.GetOrCreateCart;

public sealed class GetOrCreateCartCommandHandler : ICommandHandler<GetOrCreateCartCommand, EmployeeShoppingCartDto>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public GetOrCreateCartCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<EmployeeShoppingCartDto> Handle(GetOrCreateCartCommand command, CancellationToken cancellationToken)
    {
        CartAccessGuard.EnsureCanAccessEmployee(_currentUser, command.EmployeeId);

        var query = _dbContext.ShoppingCarts.Where(c => c.EmployeeId == command.EmployeeId && c.Status == CartStatus.Active);
        var cart = await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (cart == null)
        {
            // Create new cart
            cart = EmployeeShoppingCart.Create(
                _currentUser.GetTenant() ?? throw new InvalidOperationException("Tenant ID required"),
                command.EmployeeId);
            cart.CreatedBy = _currentUser.GetUserId().ToString();
            _dbContext.ShoppingCarts.Add(cart);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return cart.ToEmployeeShoppingCartDto();
    }
}
