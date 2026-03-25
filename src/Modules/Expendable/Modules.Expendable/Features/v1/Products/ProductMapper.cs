using FSH.Modules.Expendable.Contracts.v1.Products;
using FSH.Modules.Expendable.Domain.Products;
using System.Linq;

namespace FSH.Modules.Expendable.Features.v1.Products;

internal static class ProductMapper
{
    internal static ProductDto ToProductDto(this Product product) =>
        new(
            product.Id,
            product.SKU,
            product.Name,
            product.Description,
            product.UnitPrice,
            product.UnitOfMeasure,
            product.MinimumStockLevel,
            product.ReorderQuantity,
            product.Status.ToString(),
            product.CategoryId,
            product.SupplierId,
            product.Images?.Select(i => i.Url).ToList() ?? new List<string>(),
            product.CreatedOnUtc,
            product.CreatedBy,
            product.LastModifiedOnUtc,
            product.LastModifiedBy);
}

