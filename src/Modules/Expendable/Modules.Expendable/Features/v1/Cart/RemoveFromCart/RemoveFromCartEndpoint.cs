using FSH.Modules.Expendable.Contracts.v1.Cart;
using FSH.Framework.Shared.Identity.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Expendable.Features.v1.Cart.RemoveFromCart;

public static class RemoveFromCartEndpoint
{
    public static RouteHandlerBuilder Map(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete("/{cartId:guid}/items/{productId:guid}", RemoveItem)
            .WithName(nameof(RemoveFromCartCommand))
            .WithSummary("Remove item from cart")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(ExpendableModuleConstants.Permissions.ShoppingCarts.Edit);

    private static async Task<IResult> RemoveItem(
        Guid cartId,
        Guid productId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RemoveFromCartCommand(cartId, productId);
        await mediator.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }
}

