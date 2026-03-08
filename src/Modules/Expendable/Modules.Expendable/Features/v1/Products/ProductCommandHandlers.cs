using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Contracts.v1.Products;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Products;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Products;

public sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, ProductDto>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public CreateProductCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<ProductDto> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            _currentUser.GetTenant() ?? throw new InvalidOperationException("Tenant ID required"),
            command.SKU,
            command.Name,
            command.Description,
            command.UnitPrice,
            command.UnitOfMeasure,
            command.MinimumStockLevel,
            command.ReorderQuantity);

        product.CategoryId = command.CategoryId;
        product.SupplierId = command.SupplierId;
        product.CreatedBy = _currentUser.GetUserId().ToString();

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return product.ToProductDto();
    }
}

public sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, ProductDto>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public UpdateProductCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<ProductDto> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Product {command.Id} not found.");

        product.Update(
            command.Name,
            command.Description,
            command.UnitPrice,
            command.MinimumStockLevel,
            command.ReorderQuantity);

        product.CategoryId = command.CategoryId;
        product.SupplierId = command.SupplierId;
        product.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return product.ToProductDto();
    }
}

public sealed class ActivateProductCommandHandler : ICommandHandler<ActivateProductCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public ActivateProductCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(ActivateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Product {command.Id} not found.");

        product.Activate();
        product.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class DeactivateProductCommandHandler : ICommandHandler<DeactivateProductCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public DeactivateProductCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(DeactivateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Product {command.Id} not found.");

        product.Deactivate();
        product.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class DiscontinueProductCommandHandler : ICommandHandler<DiscontinueProductCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public DiscontinueProductCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(DiscontinueProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Product {command.Id} not found.");

        product.Discontinue();
        product.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class MarkOutOfStockCommandHandler : ICommandHandler<MarkOutOfStockCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public MarkOutOfStockCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(MarkOutOfStockCommand command, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Product {command.Id} not found.");

        product.MarkOutOfStock();
        product.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public DeleteProductCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Product {command.Id} not found.");

        product.SoftDelete(_currentUser.GetUserId().ToString());

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}



