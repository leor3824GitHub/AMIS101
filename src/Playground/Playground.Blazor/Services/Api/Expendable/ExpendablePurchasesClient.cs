using FSH.Framework.Shared.Persistence;
using FSH.Playground.Blazor.ApiClient;
using System.Net.Http.Json;
using System.Text.Json;

namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal sealed class ExpendablePurchasesClient : IExpendablePurchasesClient
{
    private readonly HttpClient _httpClient;
    private readonly IPurchasesClient _purchasesClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ExpendablePurchasesClient(HttpClient httpClient, IPurchasesClient purchasesClient)
    {
        _httpClient = httpClient;
        _purchasesClient = purchasesClient;
    }

    public async Task CreateAsync(CreatePurchaseOrderCommand command, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/expendable/purchases",
            command,
            JsonOptions,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<PagedResponse<IExpendablePurchasesClient.PurchaseDto>> SearchAsync(
        string? poNumber = null,
        string? status = null,
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (poNumber != null)
            queryParams.Add($"poNumber={Uri.EscapeDataString(poNumber)}");
        if (status != null)
            queryParams.Add($"status={Uri.EscapeDataString(status)}");
        if (pageNumber != null)
            queryParams.Add($"pageNumber={pageNumber}");
        if (pageSize != null)
            queryParams.Add($"pageSize={pageSize}");

        var url = "/api/v1/expendable/purchases/search" +
                  (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");

        var response = await _httpClient.PostAsync(new Uri(url, UriKind.Relative), null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<PagedResponse<IExpendablePurchasesClient.PurchaseDto>>(jsonContent, JsonOptions);

        return result ?? new PagedResponse<IExpendablePurchasesClient.PurchaseDto>
        {
            Items = new List<IExpendablePurchasesClient.PurchaseDto>(),
            TotalCount = 0
        };
    }

    public async Task<IExpendablePurchasesClient.PurchaseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(new Uri($"/api/v1/expendable/purchases/{id}", UriKind.Relative), cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<IExpendablePurchasesClient.PurchaseDto>(jsonContent, JsonOptions)
            ?? throw new InvalidOperationException($"Invalid response data received");
    }

    public Task SubmitAsync(Guid id, CancellationToken cancellationToken = default) =>
        _purchasesClient.SubmitAsync(id, cancellationToken);

    public Task ApproveAsync(Guid id, CancellationToken cancellationToken = default) =>
        _purchasesClient.ApproveAsync(id, cancellationToken);
}
