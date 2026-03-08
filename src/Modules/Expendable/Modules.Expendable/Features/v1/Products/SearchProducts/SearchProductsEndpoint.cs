using FSH.Modules.Expendable.Contracts.v1.Products;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Expendable.Features.v1.Products.SearchProducts;

public static class SearchProductsEndpoint
{
    public static RouteHandlerBuilder Map(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/search", Search)
            .WithName(nameof(SearchProductsQuery))
            .WithSummary("Search products")
            .Produces<PagedResponse<ProductDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequirePermission(ExpendableModuleConstants.Permissions.Products.View);

    private static async Task<IResult> Search(
        SearchProductsQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }
}

