using FSH.Modules.Expendable.Contracts.v1.Cart;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Expendable.Contracts.v1.Requests;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Expendable.Features.v1.Cart.ConvertCartToRequest;

public static class ConvertCartToSupplyRequestEndpoint
{
    public static RouteHandlerBuilder Map(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/{cartId:guid}/convert-to-request", ConvertToRequest)
            .WithName(nameof(ConvertCartToSupplyRequestCommand))
            .WithSummary("Convert shopping cart to supply request")
            .Produces<SupplyRequestDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(ExpendableModuleConstants.Permissions.SupplyRequests.Create);

    private static async Task<IResult> ConvertToRequest(
        Guid cartId,
        ConvertCartToSupplyRequestCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var cmd = command with { CartId = cartId };
        var result = await mediator.Send(cmd, cancellationToken);
        return TypedResults.Created($"/api/v1/expendable/requests/{result.Id}", result);
    }
}

