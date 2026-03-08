using FSH.Modules.Expendable.Contracts.v1.Products;
using FSH.Framework.Shared.Identity.Authorization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Expendable.Features.v1.Products.UpdateProduct;

public static class UpdateProductEndpoint
{
    public static RouteHandlerBuilder Map(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/{id:guid}", UpdateProduct)
            .WithName(nameof(UpdateProductCommand))
            .WithSummary("Update an existing product")
            .Produces<ProductDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(ExpendableModuleConstants.Permissions.Products.Update);

    private static async Task<IResult> UpdateProduct(
        Guid id,
        UpdateProductCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var cmd = command with { Id = id };
        var result = await mediator.Send(cmd, cancellationToken);
        return TypedResults.Ok(result);
    }
}

