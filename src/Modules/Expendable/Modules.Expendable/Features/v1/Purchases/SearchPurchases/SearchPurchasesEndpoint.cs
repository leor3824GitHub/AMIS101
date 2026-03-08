using FSH.Modules.Expendable.Contracts.v1.Purchases;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Expendable.Features.v1.Purchases.SearchPurchases;

public static class SearchPurchasesEndpoint
{
    public static RouteHandlerBuilder Map(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/search", Search)
            .WithName(nameof(SearchPurchasesQuery))
            .WithSummary("Search purchase orders")
            .Produces<PagedResponse<PurchaseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequirePermission(ExpendableModuleConstants.Permissions.Purchases.View);

    private static async Task<IResult> Search(
        SearchPurchasesQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }
}

