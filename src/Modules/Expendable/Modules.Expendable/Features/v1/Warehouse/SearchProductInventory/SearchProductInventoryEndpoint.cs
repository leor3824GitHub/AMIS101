using FSH.Modules.Expendable.Contracts.v1.Warehouse;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Expendable.Features.v1.Warehouse.SearchProductInventory;

public static class SearchProductInventoryEndpoint
{
    public static RouteHandlerBuilder Map(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/inventory/search", Search)
            .WithName(nameof(SearchProductInventoryQuery))
            .WithSummary("Search product inventory")
            .Produces<PagedResponse<ProductInventoryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequirePermission(ExpendableModuleConstants.Permissions.Inventory.View);

    private static async Task<IResult> Search(
        SearchProductInventoryQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }
}

