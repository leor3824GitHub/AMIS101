using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Modules.Expendable.Contracts.v1.Products;

public record ProductDto(
    Guid Id,
    string SKU,
    string Name,
    string Description,
    decimal UnitPrice,
    string UnitOfMeasure,
    int MinimumStockLevel,
    int ReorderQuantity,
    string Status,
    string? CategoryId,
    string? SupplierId,
    string? ImageUrl,
    DateTimeOffset CreatedOnUtc,
    string? CreatedBy,
    DateTimeOffset? LastModifiedOnUtc,
    string? LastModifiedBy);

public record CreateProductCommand(
    string SKU,
    string Name,
    string Description,
    decimal UnitPrice,
    string UnitOfMeasure,
    int MinimumStockLevel,
    int ReorderQuantity,
    string? CategoryId = null,
    string? SupplierId = null,
    string? ImageUrl = null) : ICommand<ProductDto>;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal UnitPrice,
    int MinimumStockLevel,
    int ReorderQuantity,
    string? CategoryId = null,
    string? SupplierId = null,
    string? ImageUrl = null) : ICommand<ProductDto>;

public record ActivateProductCommand(Guid Id) : ICommand<Unit>;

public record DeactivateProductCommand(Guid Id) : ICommand<Unit>;

public record DiscontinueProductCommand(Guid Id) : ICommand<Unit>;

public record MarkOutOfStockCommand(Guid Id) : ICommand<Unit>;

public record DeleteProductCommand(Guid Id) : ICommand<Unit>;

public record GetProductQuery(Guid Id) : IQuery<ProductDto?>;

public sealed class SearchProductsQuery : IPagedQuery, IQuery<PagedResponse<ProductDto>>
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
}

public sealed class ListActiveProductsQuery : IPagedQuery, IQuery<PagedResponse<ProductDto>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string? Sort { get; set; }
}

