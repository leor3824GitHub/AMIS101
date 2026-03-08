using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Contracts.v1.Cart;
using FSH.Modules.Expendable.Contracts.v1.Requests;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Cart;
using FSH.Modules.Expendable.Domain.Requests;
using FSH.Modules.Expendable.Features.v1.Requests;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Cart;

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

        cart.AddItem(command.ProductId, command.Quantity, command.UnitPrice);
        cart.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class UpdateCartItemQuantityCommandHandler : ICommandHandler<UpdateCartItemQuantityCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public UpdateCartItemQuantityCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(UpdateCartItemQuantityCommand command, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.ShoppingCarts
            .FirstOrDefaultAsync(c => c.Id == command.CartId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Cart {command.CartId} not found.");

        cart.UpdateItemQuantity(command.ProductId, command.NewQuantity);
        cart.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class RemoveFromCartCommandHandler : ICommandHandler<RemoveFromCartCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public RemoveFromCartCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(RemoveFromCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.ShoppingCarts
            .FirstOrDefaultAsync(c => c.Id == command.CartId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Cart {command.CartId} not found.");

        cart.RemoveItem(command.ProductId);
        cart.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class ConvertCartToSupplyRequestCommandHandler : ICommandHandler<ConvertCartToSupplyRequestCommand, SupplyRequestDto>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public ConvertCartToSupplyRequestCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<SupplyRequestDto> Handle(ConvertCartToSupplyRequestCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var cart = await _dbContext.ShoppingCarts
            .FirstOrDefaultAsync(c => c.Id == command.CartId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Cart {command.CartId} not found.");

        // Create supply request
        var requestNumber = $"REQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}";
        var request = SupplyRequest.Create(
            _currentUser.GetTenant() ?? throw new InvalidOperationException("Tenant ID required"),
            requestNumber,
            _currentUser.GetUserId().ToString(),
            command.DepartmentId,
            command.BusinessJustification,
            command.NeededByDate);

        request.CreatedBy = _currentUser.GetUserId().ToString();

        // Add items from cart
        foreach (var item in cart.Items)
        {
            request.AddItem(item.ProductId, item.Quantity);
        }

        _dbContext.SupplyRequests.Add(request);
        cart.ConvertToRequest(request.Id);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        return request.ToSupplyRequestDto();
    }
}

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

        cart.Clear();
        cart.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}



