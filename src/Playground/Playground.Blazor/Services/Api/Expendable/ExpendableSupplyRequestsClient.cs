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
        var url = QueryStringBuilder.Build(
            "/api/v1/expendable/supply-requests",
            ("status", status),
            ("employeeId", employeeId),
            ("departmentId", departmentId),
            ("pageNumber", pageNumber),
            ("pageSize", pageSize));

        var result = await _httpClient.GetFromJsonAsync<PagedResponse<IExpendableSupplyRequestsClient.SupplyRequestDto>>(
            url, JsonOptions, cancellationToken);

        return result ?? new PagedResponse<IExpendableSupplyRequestsClient.SupplyRequestDto>
        {
            Items = new List<IExpendableSupplyRequestsClient.SupplyRequestDto>(),
            TotalCount = 0
        };
    }

    public async Task<IExpendableSupplyRequestsClient.SupplyRequestDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<IExpendableSupplyRequestsClient.SupplyRequestDto>(
            $"/api/v1/expendable/supply-requests/{id}", JsonOptions, cancellationToken);

        return result ?? throw new InvalidOperationException($"Invalid response data received");
    }

    public Task SubmitAsync(Guid id, CancellationToken cancellationToken = default) =>
        _supplyRequestsClient.SubmitAsync(id, cancellationToken);

    public Task ApproveAsync(Guid id, ApproveSupplyRequestCommand command, CancellationToken cancellationToken = default) =>
        _supplyRequestsClient.ApproveAsync(id, command, cancellationToken);
}
