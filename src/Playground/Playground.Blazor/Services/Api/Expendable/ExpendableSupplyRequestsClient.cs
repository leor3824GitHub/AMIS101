using FSH.Framework.Shared.Persistence;
using FSH.Playground.Blazor.ApiClient;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal sealed class ExpendableSupplyRequestsClient : IExpendableSupplyRequestsClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public ExpendableSupplyRequestsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IExpendableSupplyRequestsClient.SupplyRequestDto> CreateAsync(CreateSupplyRequestCommand command, CancellationToken cancellationToken = default)
    {
        using var content = CreateJsonContent(command);
        var response = await _httpClient.PostAsync("/api/v1/expendable/supply-requests", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await ReadResponseAsync<IExpendableSupplyRequestsClient.SupplyRequestDto>(response, cancellationToken);
    }

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

    public async Task<PagedResponse<IExpendableSupplyRequestsClient.SupplyRequestDto>> GetEmployeeRequestsAsync(
        string employeeId,
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var url = QueryStringBuilder.Build(
            $"/api/v1/expendable/supply-requests/employee/{Uri.EscapeDataString(employeeId)}/requests",
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

    public async Task SubmitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsync($"/api/v1/expendable/supply-requests/{id}/submit", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ApproveAsync(Guid id, ApproveSupplyRequestCommand command, CancellationToken cancellationToken = default)
    {
        using var content = CreateJsonContent(command);
        var response = await _httpClient.PutAsync($"/api/v1/expendable/supply-requests/{id}/approve", content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static StringContent CreateJsonContent(object payload)
        => new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return payload ?? throw new InvalidOperationException("Response payload was empty.");
    }
}
