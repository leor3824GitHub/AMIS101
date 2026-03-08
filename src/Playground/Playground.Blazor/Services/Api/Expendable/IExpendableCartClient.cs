using FSH.Playground.Blazor.ApiClient;

namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal interface IExpendableCartClient
{
    Task GetOrCreateAsync(GetOrCreateCartCommand command, CancellationToken cancellationToken = default);

    Task AddItemAsync(Guid cartId, AddToCartCommand command, CancellationToken cancellationToken = default);

    Task GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default);

    Task ConvertToSupplyRequestAsync(
        Guid cartId,
        ConvertCartToSupplyRequestCommand command,
        CancellationToken cancellationToken = default);
}
