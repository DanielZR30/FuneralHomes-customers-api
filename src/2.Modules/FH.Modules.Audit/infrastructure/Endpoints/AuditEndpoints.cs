using FH.Modules.Audit.Application.DTOs;
using FH.Modules.Audit.Application.Queries.GetMemberAuditLog;
using FH.Modules.Audit.Application.Queries.GetSubscriptionAuditLog;
using FH.Shared.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FH.Modules.Audit.Infrastructure.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/subscriptions/{subscriptionId:guid}/audit-log")
            .WithTags("Audit");

        group.MapGet("/", async (
            Guid subscriptionId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetSubscriptionAuditLogQuery(subscriptionId),
                cancellationToken);

            return result.IsFailure ? ToError(result) : Results.Ok(result.Value);
        })
        .WithName("GetSubscriptionAuditLog")
        .WithSummary("Consultar historial cronológico inmutable de novedades de una suscripción (Módulo Audit)")
        .Produces<IReadOnlyList<AuditLogResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        routes.MapGet("/api/v1/members/{memberId:guid}/audit-log", async (
            Guid memberId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetMemberAuditLogQuery(memberId),
                cancellationToken);

            return result.IsFailure ? ToError(result) : Results.Ok(result.Value);
        })
        .WithTags("Audit")
        .WithName("GetMemberAuditLog")
        .WithSummary("Consultar historial de movimientos y coberturas de un miembro por UUID (Módulo Audit)")
        .Produces<IReadOnlyList<AuditLogResponse>>(StatusCodes.Status200OK);

        return routes;
    }

    private static IResult ToError(Result result) =>
        Results.Json(new { message = result.ErrorMessage }, statusCode: result.StatusCode);
}
