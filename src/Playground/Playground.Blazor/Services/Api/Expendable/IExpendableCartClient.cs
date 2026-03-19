namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal interface IExpendableCartClient
{
    Task<EmployeeShoppingCartDto> GetOrCreateAsync(string employeeId, CancellationToken cancellationToken = default);

    Task AddItemAsync(Guid cartId, Guid productId, int quantity, CancellationToken cancellationToken = default);

    Task<EmployeeShoppingCartDto?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default);

    Task<EmployeeShoppingCartDto?> GetByEmployeeAsync(string employeeId, CancellationToken cancellationToken = default);

    Task UpdateItemQuantityAsync(Guid cartId, Guid productId, int newQuantity, CancellationToken cancellationToken = default);

    Task RemoveItemAsync(Guid cartId, Guid productId, CancellationToken cancellationToken = default);

    Task ClearAsync(Guid cartId, CancellationToken cancellationToken = default);

    Task<IExpendableSupplyRequestsClient.SupplyRequestDto> ConvertToSupplyRequestAsync(
        Guid cartId,
        string departmentId,
        string? businessJustification = null,
        DateTimeOffset? neededByDate = null,
        CancellationToken cancellationToken = default);

    public sealed record CartItemDto(
        Guid ProductId,
        int Quantity,
        decimal UnitPrice,
        decimal LineTotal);

    public sealed record EmployeeShoppingCartDto(
        Guid Id,
        string EmployeeId,
        string Status,
        decimal CartTotal,
        int ItemCount,
        List<CartItemDto> Items,
        DateTimeOffset CreatedOnUtc);
}
