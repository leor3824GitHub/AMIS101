using FSH.Framework.Shared.Persistence;
using FSH.Playground.Blazor.ApiClient;

namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal interface IExpendablePurchasesClient
{
    Task CreateAsync(CreatePurchaseOrderCommand command, CancellationToken cancellationToken = default);

    Task<PagedResponse<PurchaseDto>> SearchAsync(
        string? poNumber = null,
        string? status = null,
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default);

    Task<PurchaseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task SubmitAsync(Guid id, CancellationToken cancellationToken = default);

    Task ApproveAsync(Guid id, CancellationToken cancellationToken = default);

    public record PurchaseDto(
        Guid Id,
        string PONumber,
        string Status,
        string SupplierName,
        string WarehouseLocationName,
        DateTime CreatedDate,
        int LineItemCount);
}
