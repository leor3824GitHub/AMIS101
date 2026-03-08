using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Contracts.v1.Products;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Products;
using Mediator;

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
