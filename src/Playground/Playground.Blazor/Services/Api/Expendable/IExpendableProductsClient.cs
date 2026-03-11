using FSH.Framework.Shared.Persistence;
using FSH.Playground.Blazor.ApiClient;

namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal interface IExpendableProductsClient
{
    Task CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default);

    Task<PagedResponse<ProductDto>> SearchAsync(
        string? keyword = null,
        string? status = null,
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default);

    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid id, UpdateProductCommand command, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResponse<ProductDto>> ListActiveAsync(
        int? pageNumber = null,
        int? pageSize = null,
        string? sort = null,
        CancellationToken cancellationToken = default);

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
        DateTimeOffset CreatedOnUtc,
        string? CreatedBy,
        DateTimeOffset? LastModifiedOnUtc,
        string? LastModifiedBy);
}
