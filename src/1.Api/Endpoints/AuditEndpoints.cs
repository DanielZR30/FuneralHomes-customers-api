using FH.Api.Customer.DTOs;
using FH.Api.Customer.MockData;

namespace FH.Api.Customer.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/subscriptions/{subscriptionId:guid}/audit-log")
            .WithTags("Audit");

        // GET /api/v1/subscriptions/{subscriptionId}/audit-log - Historial de novedades de una suscripción
        group.MapGet("/", (Guid subscriptionId, InMemoryCustomerStore store) =>
        {
            var logs = store.GetAuditLogsBySubscription(subscriptionId).Select(AuditLogResponse.FromEntity);
            return Results.Ok(logs);
        })
        .WithName("GetSubscriptionAuditLog")
        .WithSummary("Consultar historial cronológico inmutable de novedades de una suscripción (Mock en Memoria)")
        .Produces<IEnumerable<AuditLogResponse>>(StatusCodes.Status200OK);

        // GET /api/v1/members/{memberId}/audit-log - Historial de novedades de un miembro
        routes.MapGet("/api/v1/members/{memberId:guid}/audit-log", (Guid memberId, InMemoryCustomerStore store) =>
        {
            var logs = store.GetAuditLogsByMember(memberId).Select(AuditLogResponse.FromEntity);
            return Results.Ok(logs);
        })
        .WithTags("Audit")
        .WithName("GetMemberAuditLog")
        .WithSummary("Consultar historial de movimientos y coberturas de un miembro por UUID (Mock en Memoria)")
        .Produces<IEnumerable<AuditLogResponse>>(StatusCodes.Status200OK);

        return routes;
    }
}
