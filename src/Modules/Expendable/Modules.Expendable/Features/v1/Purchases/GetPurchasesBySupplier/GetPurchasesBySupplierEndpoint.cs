using FSH.Modules.Expendable.Contracts.v1.Purchases;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Persistence;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Expendable.Features.v1.Purchases.GetPurchasesBySupplier;

public static class GetPurchasesBySupplierEndpoint
{
    public static RouteHandlerBuilder Map(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/supplier/{supplierId:guid}", GetBySupplier)
            .WithName(nameof(GetPurchasesBySupplierQuery))
            .WithSummary("Get purchase orders by supplier")
            .Produces<PagedResponse<PurchaseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequirePermission(ExpendableModuleConstants.Permissions.Purchases.View);

    private static async Task<IResult> GetBySupplier(
        Guid supplierId,
        [AsParameters] GetPurchasesBySupplierQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var q = query with { SupplierId = supplierId.ToString() };
        var result = await mediator.Send(q, cancellationToken);
        return TypedResults.Ok(result);
    }
}

