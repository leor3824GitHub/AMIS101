using FSH.Framework.Shared.Persistence;
using FSH.Playground.Blazor.ApiClient;

namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal interface IExpendableWarehouseClient
{
    Task RecordInspectionAsync(RecordInspectionCommand command, CancellationToken cancellationToken = default);

    Task<InventoryDto> GetInventoryAsync(Guid productId, Guid warehouseLocationId, CancellationToken cancellationToken = default);

    Task<PagedResponse<InventoryDto>> SearchInventoryAsync(
        Guid? warehouseLocationId = null,
        string? productCode = null,
        string? productName = null,
        int? pageNumber = null,
        int? pageSize = null,
        string? sort = null,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<StockLevelDto>> GetStockLevelsAsync(
        Guid warehouseLocationId,
        int? pageNumber = null,
        int? pageSize = null,
        string? sort = null,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<RejectedInventoryDto>> GetRejectedAsync(
        Guid? warehouseLocationId = null,
        string? status = null,
        int? pageNumber = null,
        int? pageSize = null,
        string? sort = null,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<PendingInspectionDto>> GetPendingInspectionsAsync(
        Guid? warehouseLocationId = null,
        int? pageNumber = null,
        int? pageSize = null,
        string? sort = null,
        CancellationToken cancellationToken = default);

    Task ReserveAsync(Guid inventoryId, ReserveProductInventoryCommand command, CancellationToken cancellationToken = default);

    Task CancelReservationAsync(Guid inventoryId, CancelProductInventoryReservationCommand command, CancellationToken cancellationToken = default);

    Task IssueAsync(Guid inventoryId, IssueFromProductInventoryCommand command, CancellationToken cancellationToken = default);

    Task MarkRejectedReturnedAsync(Guid rejectedId, MarkRejectedInventoryReturnedCommand command, CancellationToken cancellationToken = default);

    Task MarkRejectedDisposedAsync(Guid rejectedId, MarkRejectedInventoryDisposedCommand command, CancellationToken cancellationToken = default);

    // DTOs
    public record InventoryDto(
        Guid Id,
        Guid ProductId,
        string ProductCode,
        string ProductName,
        Guid WarehouseLocationId,
        int AvailableQuantity,
        int ReservedQuantity,
        int RejectedQuantity);

    public record StockLevelDto(
        string ProductName,
        int AvailableQuantity,
        int ReservedQuantity);

    public record RejectedInventoryDto(
        string ProductName,
        int Quantity,
        string Status,
        string? RejectionReason);

    public record PendingInspectionDto(
        string PurchaseOrderNumber,
        string ProductName,
        int QuantityPendingInspection,
        DateTimeOffset? ExpectedDeliveryDate);
}
