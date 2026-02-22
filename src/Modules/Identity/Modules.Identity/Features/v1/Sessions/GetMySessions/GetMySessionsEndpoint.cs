using FSH.Framework.Shared.Identity;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Identity.Contracts.v1.Sessions.GetMySessions;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Sessions.GetMySessions;

public static class GetMySessionsEndpoint
{
    internal static RouteHandlerBuilder MapGetMySessionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/sessions/me", (CancellationToken cancellationToken, IMediator mediator) =>
            mediator.Send(new GetMySessionsQuery(), cancellationToken))
        .WithName("GetMySessions")
        .WithSummary("Get current user's sessions")
        .RequirePermission(IdentityPermissionConstants.Sessions.View)
        .WithDescription("Retrieve all active sessions for the currently authenticated user.");
    }
}
