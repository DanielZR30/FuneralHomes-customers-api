using FH.Modules.Audit.Application.DTOs;
using FH.Modules.Audit.Application.Queries.GetMemberAuditLog;
using FH.Modules.Audit.Application.Queries.GetSubscriptionAuditLog;
using FH.Shared.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FH.Customer.Api.Controllers;

[ApiController]
[Route("api/v1/subscriptions/{subscriptionId:guid}/[controller]")]
[Produces("application/json")]
public class AuditController : ControllerBase
{
    private readonly ISender _sender;

    public AuditController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySubscription(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSubscriptionAuditLogQuery(subscriptionId), cancellationToken);
        return result.IsFailure ? ToActionResult(result) : Ok(result.Value);
    }

    [HttpGet("members/{memberId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByMember(Guid subscriptionId, Guid memberId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMemberAuditLogQuery(memberId), cancellationToken);
        return result.IsFailure ? ToActionResult(result) : Ok(result.Value);
    }

    private IActionResult ToActionResult(Result result)
    {
        return StatusCode(result.StatusCode, new ProblemDetails
        {
            Status = result.StatusCode,
            Title = result.ErrorCode ?? "Error",
            Detail = result.ErrorMessage
        });
    }
}
