using FSH.Framework.Shared.Persistence;
using FSH.Playground.Blazor.ApiClient;
using System.Net.Http.Json;
using System.Text.Json;

namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal sealed class ExpendableProductsClient : IExpendableProductsClient
{
    private readonly IExpendableClient _expendableClient;
    private readonly IProductsClient _productsClient;
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ExpendableProductsClient(IExpendableClient expendableClient, IProductsClient productsClient, HttpClient httpClient)
    {
        _expendableClient = expendableClient;
        _productsClient = productsClient;
        _httpClient = httpClient;
    }

    public Task CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default) =>
        _expendableClient.ProductsPostAsync(command, cancellationToken);

    public async Task<PagedResponse<IExpendableProductsClient.ProductDto>> SearchAsync(
        string? keyword = null,
        string? status = null,
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var url = QueryStringBuilder.Build(
            "/api/v1/expendable/products",
            ("keyword", keyword),
            ("status", status),
            ("pageNumber", pageNumber),
            ("pageSize", pageSize));

        var result = await _httpClient.GetFromJsonAsync<PagedResponse<IExpendableProductsClient.ProductDto>>(
            url, JsonOptions, cancellationToken);

        return result ?? new PagedResponse<IExpendableProductsClient.ProductDto>
        {
            Items = new List<IExpendableProductsClient.ProductDto>(),
            TotalCount = 0
        };
    }

    public async Task<IExpendableProductsClient.ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<IExpendableProductsClient.ProductDto>(
            $"/api/v1/expendable/products/{id}", JsonOptions, cancellationToken);

        return result ?? throw new InvalidOperationException($"Invalid response data received");
    }

    public Task UpdateAsync(Guid id, UpdateProductCommand command, CancellationToken cancellationToken = default) =>
        _expendableClient.ProductsPutAsync(id, command, cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        _expendableClient.ProductsDeleteAsync(id, cancellationToken);

    public Task ActivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        _productsClient.ActivateAsync(id, cancellationToken);

    public Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default) =>
        _productsClient.DeactivateAsync(id, cancellationToken);

    public async Task<PagedResponse<IExpendableProductsClient.ProductDto>> ListActiveAsync(
        int? pageNumber = null,
        int? pageSize = null,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var url = QueryStringBuilder.Build(
            "/api/v1/expendable/products/active",
            ("pageNumber", pageNumber),
            ("pageSize", pageSize),
            ("sort", sort));

        var result = await _httpClient.GetFromJsonAsync<PagedResponse<IExpendableProductsClient.ProductDto>>(
            url, JsonOptions, cancellationToken);

        return result ?? new PagedResponse<IExpendableProductsClient.ProductDto>
        {
            Items = new List<IExpendableProductsClient.ProductDto>(),
            TotalCount = 0
        };
    }
}
