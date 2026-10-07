using FH.Modules.Beneficiaries.Application.Commands.AddBeneficiary;
using FH.Modules.Beneficiaries.Application.Commands.RemoveBeneficiary;
using FH.Modules.Beneficiaries.Application.Commands.UpdateBeneficiary;
using FH.Modules.Beneficiaries.Application.DTOs;
using FH.Modules.Beneficiaries.Application.Queries.GetBeneficiariesBySubscription;
using FH.Shared.Application.Common;
using FH.Shared.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FH.Customer.Api.Controllers;

[ApiController]
[Route("api/v1/subscriptions/{subscriptionId:guid}/[controller]")]
[Produces("application/json")]
public class BeneficiariesController : ControllerBase
{
    private readonly ISender _sender;

    public BeneficiariesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BeneficiaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySubscription(
        Guid subscriptionId,
        [FromQuery] BeneficiaryStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBeneficiariesBySubscriptionQuery(subscriptionId, status), cancellationToken);
        return result.IsFailure ? ToActionResult(result) : Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BeneficiaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Add(
        Guid subscriptionId,
        [FromBody] AddBeneficiaryApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddBeneficiaryCommand(
            subscriptionId,
            request.SubjectType,
            request.FirstName,
            request.BirthDate,
            request.BeneficiaryType,
            request.RelationshipType,
            request.LastName,
            request.IdentificationType,
            request.IdentificationNumber,
            request.Email,
            request.Phone);

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return ToActionResult(result);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpDelete("{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Remove(
        Guid subscriptionId,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveBeneficiaryCommand(subscriptionId, memberId), cancellationToken);
        return result.IsFailure ? ToActionResult(result) : NoContent();
    }

    [HttpPut("{memberId:guid}")]
    [ProducesResponseType(typeof(BeneficiaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        Guid subscriptionId,
        Guid memberId,
        [FromBody] UpdateBeneficiaryApiRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBeneficiaryCommand(
            subscriptionId,
            memberId,
            request.FirstName,
            request.BirthDate,
            request.LastName,
            request.IdentificationType,
            request.IdentificationNumber,
            request.Email,
            request.Phone);

        var result = await _sender.Send(command, cancellationToken);
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

public record AddBeneficiaryApiRequest(
    SubjectType SubjectType,
    string FirstName,
    DateOnly BirthDate,
    BeneficiaryType BeneficiaryType,
    RelationshipType RelationshipType,
    string? LastName = null,
    string? IdentificationType = null,
    string? IdentificationNumber = null,
    string? Email = null,
    string? Phone = null);

public record UpdateBeneficiaryApiRequest(
    string FirstName,
    DateOnly BirthDate,
    string? LastName = null,
    string? IdentificationType = null,
    string? IdentificationNumber = null,
    string? Email = null,
    string? Phone = null);
