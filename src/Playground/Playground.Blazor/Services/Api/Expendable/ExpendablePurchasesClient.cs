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
        var url = QueryStringBuilder.Build(
            "/api/v1/expendable/purchases/search",
            ("poNumber", poNumber),
            ("status", status),
            ("pageNumber", pageNumber),
            ("pageSize", pageSize));

        var response = await _httpClient.PostAsync(url, null, cancellationToken);
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
        var result = await _httpClient.GetFromJsonAsync<IExpendablePurchasesClient.PurchaseDto>(
            $"/api/v1/expendable/purchases/{id}", JsonOptions, cancellationToken);

        return result ?? throw new InvalidOperationException($"Invalid response data received");
    }

    public Task SubmitAsync(Guid id, CancellationToken cancellationToken = default) =>
        _purchasesClient.SubmitAsync(id, cancellationToken);

    public Task ApproveAsync(Guid id, CancellationToken cancellationToken = default) =>
        _purchasesClient.ApproveAsync(id, cancellationToken);
}
