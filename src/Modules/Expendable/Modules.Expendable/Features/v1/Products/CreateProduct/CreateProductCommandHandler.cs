using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Contracts.v1.Products;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Products;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Products.CreateProduct;

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
        var skuInUse = await _dbContext.Products
            .IgnoreQueryFilters()
            .AnyAsync(p => p.TenantId == (_currentUser.GetTenant() ?? string.Empty) && p.SKU == command.SKU, cancellationToken)
            .ConfigureAwait(false);

        if (skuInUse)
        {
            throw new FluentValidation.ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(command.SKU), "A product with this SKU already exists.")
            ]);
        }

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
        product.ImageUrl = command.ImageUrl;
        product.CreatedBy = _currentUser.GetUserId().ToString();

        _dbContext.Products.Add(product);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when ((ex.InnerException?.Message?.Contains("IX_Products_TenantId_SKU", StringComparison.OrdinalIgnoreCase) ?? false)
            || (ex.InnerException?.Message?.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            throw new FluentValidation.ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(nameof(command.SKU), "A product with this SKU already exists.")
            ]);
        }

        return product.ToProductDto();
    }
}
