using Microsoft.AspNetCore.Mvc;
using FH.Customers.Application.Audit.DTOs;
using FH.Customers.Application.Audit.Queries.GetMemberAuditLog;
using FH.Customers.Application.Audit.Queries.GetSubscriptionAuditLog;

namespace FH.Customers.Api.Controllers;

[Tags("Audit")]
public class AuditController : ApiControllerBase
{
    /// <summary>Consultar historial cronológico inmutable de novedades de una suscripción (Módulo Audit)</summary>
    [HttpGet("api/v1/subscriptions/{subscriptionId:guid}/audit-log")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySubscription(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSubscriptionAuditLogQuery(subscriptionId), cancellationToken);

        return result.IsFailure ? ToError(result) : Ok(result.Value);
    }

    /// <summary>Consultar historial de movimientos y coberturas de un miembro por UUID (Módulo Audit)</summary>
    [HttpGet("api/v1/members/{memberId:guid}/audit-log")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByMember(Guid memberId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMemberAuditLogQuery(memberId), cancellationToken);

        return result.IsFailure ? ToError(result) : Ok(result.Value);
    }
}
