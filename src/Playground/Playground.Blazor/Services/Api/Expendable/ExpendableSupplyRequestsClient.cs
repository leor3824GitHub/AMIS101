using FSH.Framework.Shared.Persistence;
using FSH.Playground.Blazor.ApiClient;
using System.Net.Http.Json;
using System.Text.Json;

namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal sealed class ExpendableSupplyRequestsClient : IExpendableSupplyRequestsClient
{
    private readonly HttpClient _httpClient;
    private readonly IExpendableClient _expendableClient;
    private readonly ISupply_requestsClient _supplyRequestsClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ExpendableSupplyRequestsClient(HttpClient httpClient, IExpendableClient expendableClient, ISupply_requestsClient supplyRequestsClient)
    {
        _httpClient = httpClient;
        _expendableClient = expendableClient;
        _supplyRequestsClient = supplyRequestsClient;
    }

    public Task CreateAsync(CreateSupplyRequestCommand command, CancellationToken cancellationToken = default) =>
        _expendableClient.SupplyRequestsPostAsync(command, cancellationToken);

    public async Task<PagedResponse<IExpendableSupplyRequestsClient.SupplyRequestDto>> SearchAsync(
        string? status = null,
        string? employeeId = null,
        string? departmentId = null,
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (status != null)
            queryParams.Add($"status={Uri.EscapeDataString(status)}");
        if (employeeId != null)
            queryParams.Add($"employeeId={Uri.EscapeDataString(employeeId)}");
        if (departmentId != null)
            queryParams.Add($"departmentId={Uri.EscapeDataString(departmentId)}");
        if (pageNumber != null)
            queryParams.Add($"pageNumber={pageNumber}");
        if (pageSize != null)
            queryParams.Add($"pageSize={pageSize}");

        var url = "/api/v1/expendable/supply-requests" +
                  (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");

        var response = await _httpClient.GetAsync(new Uri(url, UriKind.Relative), cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<PagedResponse<IExpendableSupplyRequestsClient.SupplyRequestDto>>(jsonContent, JsonOptions);

        return result ?? new PagedResponse<IExpendableSupplyRequestsClient.SupplyRequestDto>
        {
            Items = new List<IExpendableSupplyRequestsClient.SupplyRequestDto>(),
            TotalCount = 0
        };
    }

    public async Task<IExpendableSupplyRequestsClient.SupplyRequestDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(new Uri($"/api/v1/expendable/supply-requests/{id}", UriKind.Relative), cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<IExpendableSupplyRequestsClient.SupplyRequestDto>(jsonContent, JsonOptions)
            ?? throw new InvalidOperationException($"Invalid response data received");
    }

    public Task SubmitAsync(Guid id, CancellationToken cancellationToken = default) =>
        _supplyRequestsClient.SubmitAsync(id, cancellationToken);

    public Task ApproveAsync(Guid id, ApproveSupplyRequestCommand command, CancellationToken cancellationToken = default) =>
        _supplyRequestsClient.ApproveAsync(id, command, cancellationToken);
}
