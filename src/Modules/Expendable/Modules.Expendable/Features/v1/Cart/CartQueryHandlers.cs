using FSH.Modules.Expendable.Contracts.v1.Cart;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Cart;
using FSH.Framework.Core.Context;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Cart;

public sealed class GetEmployeeCartQueryHandler : IQueryHandler<GetEmployeeCartQuery, EmployeeShoppingCartDto?>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public GetEmployeeCartQueryHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<EmployeeShoppingCartDto?> Handle(GetEmployeeCartQuery query, CancellationToken cancellationToken)
    {
        CartAccessGuard.EnsureCanAccessEmployee(_currentUser, query.EmployeeId);

        var cart = await _dbContext.ShoppingCarts
            .Where(c => c.EmployeeId == query.EmployeeId && c.Status == CartStatus.Active)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return cart?.ToEmployeeShoppingCartDto();
    }
}


