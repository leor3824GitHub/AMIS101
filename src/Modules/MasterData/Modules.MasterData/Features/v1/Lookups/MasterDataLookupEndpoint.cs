using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.MasterData.Contracts.v1.References;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.MasterData.Features.v1.Lookups;

public static class MasterDataLookupEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/employees", SearchEmployees)
            .WithName(nameof(SearchEmployeeReferencesQuery))
            .WithSummary("Search employee references")
            .Produces<PagedResponse<EmployeeReferenceDto>>(StatusCodes.Status200OK)
            .RequirePermission(MasterDataModuleConstants.Permissions.Lookup.View);

        endpoints.MapGet("/employees/{id:guid}", GetEmployeeById)
            .WithName(nameof(GetEmployeeReferenceByIdQuery))
            .WithSummary("Get employee reference by id")
            .Produces<EmployeeReferenceDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(MasterDataModuleConstants.Permissions.Lookup.View);

        endpoints.MapGet("/employees/by-identity/{identityUserId}", GetEmployeeByIdentity)
            .WithName(nameof(GetEmployeeReferenceByIdentityUserIdQuery))
            .WithSummary("Get employee reference by identity user id")
            .Produces<EmployeeReferenceDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(MasterDataModuleConstants.Permissions.Lookup.View);

        endpoints.MapGet("/offices", ListOffices)
            .WithName(nameof(ListOfficeReferencesQuery))
            .WithSummary("List office references")
            .Produces<IReadOnlyList<OfficeReferenceDto>>(StatusCodes.Status200OK)
            .RequirePermission(MasterDataModuleConstants.Permissions.Lookup.View);

        endpoints.MapGet("/departments", ListDepartments)
            .WithName(nameof(ListDepartmentReferencesQuery))
            .WithSummary("List department references")
            .Produces<IReadOnlyList<DepartmentReferenceDto>>(StatusCodes.Status200OK)
            .RequirePermission(MasterDataModuleConstants.Permissions.Lookup.View);

        endpoints.MapGet("/positions", ListPositions)
            .WithName(nameof(ListPositionReferencesQuery))
            .WithSummary("List position references")
            .Produces<IReadOnlyList<PositionReferenceDto>>(StatusCodes.Status200OK)
            .RequirePermission(MasterDataModuleConstants.Permissions.Lookup.View);

        endpoints.MapGet("/unit-of-measures", ListUnitOfMeasures)
            .WithName(nameof(ListUnitOfMeasureReferencesQuery))
            .WithSummary("List unit of measure references")
            .Produces<IReadOnlyList<UnitOfMeasureReferenceDto>>(StatusCodes.Status200OK)
            .RequirePermission(MasterDataModuleConstants.Permissions.Lookup.View);
    }

    private static async Task<IResult> SearchEmployees(
        [AsParameters] SearchEmployeeReferencesQuery query,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetEmployeeById(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeReferenceByIdQuery(id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> GetEmployeeByIdentity(
        string identityUserId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeReferenceByIdentityUserIdQuery(identityUserId), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> ListOffices(
        [FromQuery] bool includeInactive,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListOfficeReferencesQuery(includeInactive), cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> ListDepartments(
        [FromQuery] bool includeInactive,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListDepartmentReferencesQuery(includeInactive), cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> ListPositions(
        [FromQuery] bool includeInactive,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListPositionReferencesQuery(includeInactive), cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> ListUnitOfMeasures(
        [FromQuery] bool includeInactive,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListUnitOfMeasureReferencesQuery(includeInactive), cancellationToken);
        return TypedResults.Ok(result);
    }
}


