using FSH.Playground.Blazor.ApiClient;

namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal sealed class ExpendableCartClient : IExpendableCartClient
{
    private readonly IExpendableClient _expendableClient;
    private readonly ICartClient _cartClient;

    public ExpendableCartClient(IExpendableClient expendableClient, ICartClient cartClient)
    {
        _expendableClient = expendableClient;
        _cartClient = cartClient;
    }

    public Task GetOrCreateAsync(GetOrCreateCartCommand command, CancellationToken cancellationToken = default) =>
        _cartClient.GetOrCreateAsync(command, cancellationToken);

    public Task AddItemAsync(Guid cartId, AddToCartCommand command, CancellationToken cancellationToken = default) =>
        _cartClient.AddItemAsync(cartId, command, cancellationToken);

    public Task GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default) =>
        _expendableClient.CartAsync(cartId, cancellationToken);

    public Task ConvertToSupplyRequestAsync(
        Guid cartId,
        ConvertCartToSupplyRequestCommand command,
        CancellationToken cancellationToken = default) =>
        _cartClient.ConvertToRequestAsync(cartId, command, cancellationToken);
}
