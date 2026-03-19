using FSH.Framework.Shared.Persistence;
using FSH.Playground.Blazor.ApiClient;
using System.Net;
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

    public async Task CreateAsync(
        CreateProductCommand command,
        IReadOnlyList<string>? imageUrls = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            sku = command.Sku,
            name = command.Name,
            description = command.Description,
            unitPrice = command.UnitPrice,
            unitOfMeasure = command.UnitOfMeasure,
            minimumStockLevel = command.MinimumStockLevel,
            reorderQuantity = command.ReorderQuantity,
            categoryId = command.CategoryId,
            supplierId = command.SupplierId,
            imageUrls = imageUrls
        };

        using var response = await _httpClient.PostAsJsonAsync("/api/v1/expendable/products", payload, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<PagedResponse<IExpendableProductsClient.ProductDto>> SearchAsync(
        string? keyword = null,
        string? status = null,
        int? pageNumber = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var getUrl = QueryStringBuilder.Build(
            "/api/v1/expendable/products",
            ("keyword", keyword),
            ("status", status),
            ("pageNumber", pageNumber),
            ("pageSize", pageSize));

        using var getResponse = await _httpClient.GetAsync(getUrl, cancellationToken);
        if (getResponse.IsSuccessStatusCode)
        {
            var getResult = await getResponse.Content.ReadFromJsonAsync<PagedResponse<IExpendableProductsClient.ProductDto>>(JsonOptions, cancellationToken);
            return getResult ?? EmptySearchResult();
        }

        // Backward-compatible fallback for APIs still exposing POST /search.
        if (getResponse.StatusCode == HttpStatusCode.NotFound)
        {
            var searchBody = new { keyword, status, pageNumber, pageSize };
            using var postResponse = await _httpClient.PostAsJsonAsync(
                "/api/v1/expendable/products/search",
                searchBody,
                JsonOptions,
                cancellationToken);

            if (postResponse.IsSuccessStatusCode)
            {
                var postResult = await postResponse.Content.ReadFromJsonAsync<PagedResponse<IExpendableProductsClient.ProductDto>>(JsonOptions, cancellationToken);
                return postResult ?? EmptySearchResult();
            }

            postResponse.EnsureSuccessStatusCode();
        }

        throw new HttpRequestException(
            $"Failed to load products from '{getUrl}'. " +
            $"BaseAddress='{_httpClient.BaseAddress}', StatusCode={(int)getResponse.StatusCode} ({getResponse.StatusCode}).");
    }

    private static PagedResponse<IExpendableProductsClient.ProductDto> EmptySearchResult() =>
        new()
        {
            Items = new List<IExpendableProductsClient.ProductDto>(),
            TotalCount = 0
        };

    public async Task<IExpendableProductsClient.ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<IExpendableProductsClient.ProductDto>(
            $"/api/v1/expendable/products/{id}", JsonOptions, cancellationToken);

        return result ?? throw new InvalidOperationException($"Invalid response data received");
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateProductCommand command,
        IReadOnlyList<string>? imageUrls = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            id,
            name = command.Name,
            description = command.Description,
            unitPrice = command.UnitPrice,
            minimumStockLevel = command.MinimumStockLevel,
            reorderQuantity = command.ReorderQuantity,
            categoryId = command.CategoryId,
            supplierId = command.SupplierId,
            imageUrls = imageUrls
        };

        using var response = await _httpClient.PutAsJsonAsync($"/api/v1/expendable/products/{id}", payload, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

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

        return result ?? EmptySearchResult();
    }
}
