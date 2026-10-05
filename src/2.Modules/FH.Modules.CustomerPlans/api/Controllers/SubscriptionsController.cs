using FH.Modules.CustomerPlans.Api.Requests;
using FH.Modules.CustomerPlans.Application.Commands.CancelSubscription;
using FH.Modules.CustomerPlans.Application.Commands.SubscribeCustomer;
using FH.Modules.CustomerPlans.Application.Commands.UpdateMaxBeneficiaries;
using FH.Modules.CustomerPlans.Application.DTOs;
using FH.Modules.CustomerPlans.Application.Queries.GetCustomerSubscriptions;
using FH.Modules.CustomerPlans.Application.Queries.GetSubscriptionById;
using FH.Modules.CustomerPlans.Application.Queries.GetSubscriptionCapacity;
using FH.Shared.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FH.Modules.CustomerPlans.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Produces("application/json")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Suscribe a un cliente titular a un plan funerario con límite de cupos (RN-01 a RN-08).
    /// </summary>
    [HttpPost("subscriptions")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SubscribeCustomer(
        [FromBody] CreateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubscribeCustomerCommand(
            request.CustomerId,
            request.ExternalPlanId,
            request.MaxBeneficiaries,
            request.StartDate,
            request.EndDate);

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return ToProblemDetails(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            result.Value);
    }

    /// <summary>
    /// Consulta todas las suscripciones asociadas a un cliente titular.
    /// </summary>
    [HttpGet("customers/{customerId:guid}/subscriptions")]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomerId(
        [FromRoute] Guid customerId,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerSubscriptionsQuery(customerId);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene el detalle de una suscripción por su identificador único.
    /// </summary>
    [HttpGet("subscriptions/{id:guid}")]
    [ActionName(nameof(GetById))]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetSubscriptionByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return ToProblemDetails(result);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Cancela una suscripción exequial activa o suspendida (RN-04).
    /// </summary>
    [HttpPatch("subscriptions/{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelSubscription(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CancelSubscriptionCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return ToProblemDetails(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Modifica el cupo máximo de beneficiarios de una suscripción activa (RN-05).
    /// </summary>
    [HttpPatch("subscriptions/{id:guid}/max-beneficiaries")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateMaxBeneficiaries(
        [FromRoute] Guid id,
        [FromBody] UpdateMaxBeneficiariesRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMaxBeneficiariesCommand(id, request.MaxBeneficiaries);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return ToProblemDetails(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Consulta el estado y cupos disponibles de una suscripción para integración con Módulo 3.
    /// </summary>
    [HttpGet("subscriptions/{id:guid}/capacity")]
    [ProducesResponseType(typeof(SubscriptionCapacityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCapacity(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetSubscriptionCapacityQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return ToProblemDetails(result);
        }

        return Ok(result.Value);
    }

    private ObjectResult ToProblemDetails(Result result)
    {
        var problemDetails = new ProblemDetails
        {
            Status = result.StatusCode,
            Title = result.ErrorCode ?? "Error en la operación",
            Detail = result.ErrorMessage
        };

        return StatusCode(result.StatusCode, problemDetails);
    }
}
