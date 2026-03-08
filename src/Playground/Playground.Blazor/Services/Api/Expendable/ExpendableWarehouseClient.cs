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
        var response = await _httpClient.GetAsync(new Uri($"/api/v1/warehouse/inventory/{productId}/{warehouseLocationId}", UriKind.Relative), cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<IExpendableWarehouseClient.InventoryDto>(jsonContent, JsonOptions)
            ?? throw new InvalidOperationException($"Invalid response data received");
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
        var queryParams = new List<string>();
        if (warehouseLocationId != null)
            queryParams.Add($"warehouseLocationId={warehouseLocationId}");
        if (productCode != null)
            queryParams.Add($"productCode={Uri.EscapeDataString(productCode)}");
        if (productName != null)
            queryParams.Add($"productName={Uri.EscapeDataString(productName)}");
        if (pageNumber != null)
            queryParams.Add($"pageNumber={pageNumber}");
        if (pageSize != null)
            queryParams.Add($"pageSize={pageSize}");
        if (sort != null)
            queryParams.Add($"sort={Uri.EscapeDataString(sort)}");

        var url = "/api/v1/warehouse/inventory" +
                  (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");

        var response = await _httpClient.GetAsync(new Uri(url, UriKind.Relative), cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<PagedResponse<IExpendableWarehouseClient.InventoryDto>>(jsonContent, JsonOptions);

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
        var queryParams = new List<string>();
        queryParams.Add($"warehouseLocationId={warehouseLocationId}");
        if (pageNumber != null)
            queryParams.Add($"pageNumber={pageNumber}");
        if (pageSize != null)
            queryParams.Add($"pageSize={pageSize}");
        if (sort != null)
            queryParams.Add($"sort={Uri.EscapeDataString(sort)}");

        var url = "/api/v1/warehouse/stock-levels?" + string.Join("&", queryParams);

        var response = await _httpClient.GetAsync(new Uri(url, UriKind.Relative), cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<PagedResponse<IExpendableWarehouseClient.StockLevelDto>>(jsonContent, JsonOptions);

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
        var queryParams = new List<string>();
        if (warehouseLocationId != null)
            queryParams.Add($"warehouseLocationId={warehouseLocationId}");
        if (status != null)
            queryParams.Add($"status={Uri.EscapeDataString(status)}");
        if (pageNumber != null)
            queryParams.Add($"pageNumber={pageNumber}");
        if (pageSize != null)
            queryParams.Add($"pageSize={pageSize}");
        if (sort != null)
            queryParams.Add($"sort={Uri.EscapeDataString(sort)}");

        var url = "/api/v1/warehouse/rejected" +
                  (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");

        var response = await _httpClient.GetAsync(new Uri(url, UriKind.Relative), cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<PagedResponse<IExpendableWarehouseClient.RejectedInventoryDto>>(jsonContent, JsonOptions);

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
        var queryParams = new List<string>();
        if (warehouseLocationId != null)
            queryParams.Add($"warehouseLocationId={warehouseLocationId}");
        if (pageNumber != null)
            queryParams.Add($"pageNumber={pageNumber}");
        if (pageSize != null)
            queryParams.Add($"pageSize={pageSize}");
        if (sort != null)
            queryParams.Add($"sort={Uri.EscapeDataString(sort)}");

        var url = "/api/v1/warehouse/pending-inspections" +
                  (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");

        var response = await _httpClient.GetAsync(new Uri(url, UriKind.Relative), cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<PagedResponse<IExpendableWarehouseClient.PendingInspectionDto>>(jsonContent, JsonOptions);

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
