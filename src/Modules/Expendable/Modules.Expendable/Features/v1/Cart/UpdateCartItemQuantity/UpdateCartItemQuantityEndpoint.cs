using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Expendable.Contracts.v1.Cart;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Expendable.Features.v1.Cart.UpdateCartItemQuantity;

public static class UpdateCartItemQuantityEndpoint
{
    private sealed record UpdateCartItemQuantityRequest(int NewQuantity);

    public static RouteHandlerBuilder Map(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/{cartId:guid}/items/{productId:guid}", UpdateQuantity)
            .WithName(nameof(UpdateCartItemQuantityCommand))
            .WithSummary("Update cart item quantity")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(ExpendableModuleConstants.Permissions.ShoppingCarts.Edit);

    private static async Task<IResult> UpdateQuantity(
        Guid cartId,
        Guid productId,
        UpdateCartItemQuantityRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCartItemQuantityCommand(cartId, productId, request.NewQuantity);
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }
}
