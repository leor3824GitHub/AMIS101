using FSH.Modules.Expendable.Contracts.v1.Purchases;
using FSH.Framework.Shared.Identity.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Expendable.Features.v1.Purchases.CreatePurchaseOrder;

public static class CreatePurchaseOrderEndpoint
{
    public static RouteHandlerBuilder Map(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/", CreatePurchaseOrder)
            .WithName(nameof(CreatePurchaseOrderCommand))
            .WithSummary("Create a new purchase order")
            .Produces<PurchaseDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequirePermission(ExpendableModuleConstants.Permissions.Purchases.Create);

    private static async Task<IResult> CreatePurchaseOrder(
        CreatePurchaseOrderCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/v1/expendable/purchases/{result.Id}", result);
    }
}

