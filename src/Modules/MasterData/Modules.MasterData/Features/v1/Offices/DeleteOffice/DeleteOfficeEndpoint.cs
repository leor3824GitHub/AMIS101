using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.MasterData.Contracts.v1.References;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.MasterData.Features.v1.Offices.DeleteOffice;

public static class DeleteOfficeEndpoint
{
    public static RouteHandlerBuilder Map(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapDelete("/{id:guid}", DeleteOffice)
            .WithName(nameof(DeleteOfficeCommand))
            .WithSummary("Delete office")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(MasterDataModuleConstants.Permissions.Offices.Delete);

    private static async Task<IResult> DeleteOffice(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteOfficeCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
