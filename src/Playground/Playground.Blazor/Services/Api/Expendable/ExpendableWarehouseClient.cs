using FSH.Framework.Shared.Persistence;
using FSH.Playground.Blazor.ApiClient;
using System.Net.Http.Json;
using System.Text.Json;

namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal sealed class ExpendableWarehouseClient : IExpendableWarehouseClient
{
    private readonly IWarehouseClient _warehouseClient;
    private readonly IInventoryClient _inventoryClient;
    private readonly IRejectedClient _rejectedClient;
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ExpendableWarehouseClient(
        HttpClient httpClient,
        IWarehouseClient warehouseClient,
        IInventoryClient inventoryClient,
        IRejectedClient rejectedClient)
    {
        _httpClient = httpClient;
        _warehouseClient = warehouseClient;
        _inventoryClient = inventoryClient;
        _rejectedClient = rejectedClient;
    }

    public Task RecordInspectionAsync(RecordInspectionCommand command, CancellationToken cancellationToken = default) =>
        _warehouseClient.InspectionsAsync(command, cancellationToken);

    public async Task<IExpendableWarehouseClient.InventoryDto> GetInventoryAsync(Guid productId, Guid warehouseLocationId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<IExpendableWarehouseClient.InventoryDto>(
            $"/api/v1/warehouse/inventory/{productId}/{warehouseLocationId}", JsonOptions, cancellationToken);

        return result ?? throw new InvalidOperationException($"Invalid response data received");
    }

    public async Task<PagedResponse<IExpendableWarehouseClient.InventoryDto>> SearchInventoryAsync(
        Guid? warehouseLocationId = null,
        string? productCode = null,
        string? productName = null,
        int? pageNumber = null,
        int? pageSize = null,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var url = QueryStringBuilder.Build(
            "/api/v1/warehouse/inventory",
            ("warehouseLocationId", warehouseLocationId),
            ("productCode", productCode),
            ("productName", productName),
            ("pageNumber", pageNumber),
            ("pageSize", pageSize),
            ("sort", sort));

        var result = await _httpClient.GetFromJsonAsync<PagedResponse<IExpendableWarehouseClient.InventoryDto>>(
            url, JsonOptions, cancellationToken);

        return result ?? new PagedResponse<IExpendableWarehouseClient.InventoryDto>
        {
            Items = new List<IExpendableWarehouseClient.InventoryDto>(),
            TotalCount = 0
        };
    }

    public async Task<PagedResponse<IExpendableWarehouseClient.StockLevelDto>> GetStockLevelsAsync(
        Guid warehouseLocationId,
        int? pageNumber = null,
        int? pageSize = null,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var url = QueryStringBuilder.Build(
            "/api/v1/warehouse/stock-levels",
            ("warehouseLocationId", warehouseLocationId),
            ("pageNumber", pageNumber),
            ("pageSize", pageSize),
            ("sort", sort));

        var result = await _httpClient.GetFromJsonAsync<PagedResponse<IExpendableWarehouseClient.StockLevelDto>>(
            url, JsonOptions, cancellationToken);

        return result ?? new PagedResponse<IExpendableWarehouseClient.StockLevelDto>
        {
            Items = new List<IExpendableWarehouseClient.StockLevelDto>(),
            TotalCount = 0
        };
    }

    public async Task<PagedResponse<IExpendableWarehouseClient.RejectedInventoryDto>> GetRejectedAsync(
        Guid? warehouseLocationId = null,
        string? status = null,
        int? pageNumber = null,
        int? pageSize = null,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var url = QueryStringBuilder.Build(
            "/api/v1/warehouse/rejected",
            ("warehouseLocationId", warehouseLocationId),
            ("status", status),
            ("pageNumber", pageNumber),
            ("pageSize", pageSize),
            ("sort", sort));

        var result = await _httpClient.GetFromJsonAsync<PagedResponse<IExpendableWarehouseClient.RejectedInventoryDto>>(
            url, JsonOptions, cancellationToken);

        return result ?? new PagedResponse<IExpendableWarehouseClient.RejectedInventoryDto>
        {
            Items = new List<IExpendableWarehouseClient.RejectedInventoryDto>(),
            TotalCount = 0
        };
    }

    public async Task<PagedResponse<IExpendableWarehouseClient.PendingInspectionDto>> GetPendingInspectionsAsync(
        Guid? warehouseLocationId = null,
        int? pageNumber = null,
        int? pageSize = null,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var url = QueryStringBuilder.Build(
            "/api/v1/warehouse/pending-inspections",
            ("warehouseLocationId", warehouseLocationId),
            ("pageNumber", pageNumber),
            ("pageSize", pageSize),
            ("sort", sort));

        var result = await _httpClient.GetFromJsonAsync<PagedResponse<IExpendableWarehouseClient.PendingInspectionDto>>(
            url, JsonOptions, cancellationToken);

        return result ?? new PagedResponse<IExpendableWarehouseClient.PendingInspectionDto>
        {
            Items = new List<IExpendableWarehouseClient.PendingInspectionDto>(),
            TotalCount = 0
        };
    }

    public Task ReserveAsync(Guid inventoryId, ReserveProductInventoryCommand command, CancellationToken cancellationToken = default) =>
        _inventoryClient.ReserveAsync(inventoryId, command, cancellationToken);

    public Task CancelReservationAsync(Guid inventoryId, CancelProductInventoryReservationCommand command, CancellationToken cancellationToken = default) =>
        _inventoryClient.CancelReservationAsync(inventoryId, command, cancellationToken);

    public Task IssueAsync(Guid inventoryId, IssueFromProductInventoryCommand command, CancellationToken cancellationToken = default) =>
        _inventoryClient.IssueAsync(inventoryId, command, cancellationToken);

    public Task MarkRejectedReturnedAsync(Guid rejectedId, MarkRejectedInventoryReturnedCommand command, CancellationToken cancellationToken = default) =>
        _rejectedClient.ReturnedAsync(rejectedId, command, cancellationToken);

    public Task MarkRejectedDisposedAsync(Guid rejectedId, MarkRejectedInventoryDisposedCommand command, CancellationToken cancellationToken = default) =>
        _rejectedClient.DisposedAsync(rejectedId, command, cancellationToken);
}
