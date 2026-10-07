using Microsoft.AspNetCore.Mvc;
using static FH.Customers.Application.Beneficiaries.DTOs.BeneficiaryResponse;
using FH.Customers.Application.Beneficiaries.Commands.AddBeneficiary;
using FH.Customers.Application.Beneficiaries.Commands.RemoveBeneficiary;
using FH.Customers.Application.Beneficiaries.Commands.UpdateBeneficiary;
using FH.Customers.Application.Beneficiaries.DTOs;
using FH.Customers.Application.Beneficiaries.Queries.GetBeneficiariesBySubscription;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Api.Controllers;

[Route("api/v1/subscriptions/{subscriptionId:guid}/beneficiaries")]
[Tags("Beneficiaries")]
public class BeneficiariesController : ApiControllerBase
{
    /// <summary>Incorporar un beneficiario (persona o mascota) a una suscripción activa</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BeneficiaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Add(
        Guid subscriptionId,
        [FromBody] AddBeneficiaryRequest request,
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

        var result = await Mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return ToError(result);
        }

        return Created(
            $"/api/v1/subscriptions/{subscriptionId}/beneficiaries/{result.Value.MemberId}",
            result.Value);
    }

    /// <summary>Listar los beneficiarios de una suscripción (con edad derivada y parentesco)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BeneficiaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySubscription(
        Guid subscriptionId,
        [FromQuery] BeneficiaryStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetBeneficiariesBySubscriptionQuery(subscriptionId, status),
            cancellationToken);

        return result.IsFailure ? ToError(result) : Ok(result.Value);
    }

    /// <summary>Desafiliar a un beneficiario de una suscripción funeraria</summary>
    [HttpDelete("{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Remove(
        Guid subscriptionId,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new RemoveBeneficiaryCommand(subscriptionId, memberId),
            cancellationToken);

        return result.IsFailure ? ToError(result) : NoContent();
    }

    /// <summary>Actualizar los datos de un beneficiario activo (el tipo de sujeto y el parentesco no cambian)</summary>
    [HttpPut("{memberId:guid}")]
    [ProducesResponseType(typeof(BeneficiaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid subscriptionId,
        Guid memberId,
        [FromBody] UpdateBeneficiaryRequest request,
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

        var result = await Mediator.Send(command, cancellationToken);

        return result.IsFailure ? ToError(result) : Ok(result.Value);
    }
}
