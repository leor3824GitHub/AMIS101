using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal sealed class ExpendableCartClient : IExpendableCartClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public ExpendableCartClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IExpendableCartClient.EmployeeShoppingCartDto> GetOrCreateAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"api/v1/expendable/cart/employee/{Uri.EscapeDataString(employeeId)}/cart", null, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await ReadResponseAsync<IExpendableCartClient.EmployeeShoppingCartDto>(response, cancellationToken);
    }

    public async Task AddItemAsync(Guid cartId, Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        using var content = CreateJsonContent(new { productId, quantity });
        var response = await _httpClient.PostAsync($"api/v1/expendable/cart/{cartId}/items", content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IExpendableCartClient.EmployeeShoppingCartDto?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/expendable/cart/{cartId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadResponseAsync<IExpendableCartClient.EmployeeShoppingCartDto>(response, cancellationToken);
    }

    public async Task<IExpendableCartClient.EmployeeShoppingCartDto?> GetByEmployeeAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/expendable/cart/employee/{Uri.EscapeDataString(employeeId)}/cart", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await ReadResponseAsync<IExpendableCartClient.EmployeeShoppingCartDto>(response, cancellationToken);
    }

    public async Task UpdateItemQuantityAsync(Guid cartId, Guid productId, int newQuantity, CancellationToken cancellationToken = default)
    {
        using var content = CreateJsonContent(new { newQuantity });
        var response = await _httpClient.PutAsync($"api/v1/expendable/cart/{cartId}/items/{productId}", content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveItemAsync(Guid cartId, Guid productId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/expendable/cart/{cartId}/items/{productId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ClearAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/v1/expendable/cart/{cartId}/clear", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IExpendableSupplyRequestsClient.SupplyRequestDto> ConvertToSupplyRequestAsync(
        Guid cartId,
        string departmentId,
        string? businessJustification = null,
        DateTimeOffset? neededByDate = null,
        CancellationToken cancellationToken = default)
    {
        using var content = CreateJsonContent(new
        {
            departmentId,
            businessJustification,
            neededByDate
        });

        var response = await _httpClient.PostAsync($"api/v1/expendable/cart/{cartId}/convert-to-request", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await ReadResponseAsync<IExpendableSupplyRequestsClient.SupplyRequestDto>(response, cancellationToken);
    }

    private static StringContent CreateJsonContent(object payload)
        => new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return payload ?? throw new InvalidOperationException("Response payload was empty.");
    }
}
