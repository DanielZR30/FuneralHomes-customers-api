using Microsoft.AspNetCore.Mvc;
using FH.Customers.Api.Requests;
using FH.Customers.Application.Common;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.Commands.CancelSubscription;
using FH.Customers.Application.CustomerPlans.Commands.SubscribeCustomer;
using FH.Customers.Application.CustomerPlans.Commands.UpdateMaxBeneficiaries;
using FH.Customers.Application.CustomerPlans.DTOs;
using FH.Customers.Application.CustomerPlans.Queries.GetCustomerSubscriptions;
using FH.Customers.Application.CustomerPlans.Queries.GetSubscriptionById;
using FH.Customers.Application.CustomerPlans.Queries.GetSubscriptionCapacity;

namespace FH.Customers.Api.Controllers;

[Route("api/v1/subscriptions")]
[Tags("Subscriptions")]
public class SubscriptionsController : ApiControllerBase
{
    /// <summary>Suscribir cliente titular a un plan funerario con límite de cupos</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubscribeCustomerCommand(
            request.CustomerId,
            request.ExternalPlanId,
            request.MaxBeneficiaries,
            request.StartDate,
            request.EndDate);

        var result = await Mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return ToErrorWithCode(result);
        }

        return Created($"/api/v1/subscriptions/{result.Value.Id}", result.Value);
    }

    /// <summary>Consultar detalle de una suscripción funeraria por ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSubscriptionByIdQuery(id), cancellationToken);

        return result.IsFailure ? ToErrorWithCode(result) : Ok(result.Value);
    }

    /// <summary>Cancelar una suscripción de previsión funeraria</summary>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new CancelSubscriptionCommand(id), cancellationToken);

        return result.IsFailure ? ToErrorWithCode(result) : NoContent();
    }

    /// <summary>Modificar el cupo máximo de beneficiarios de una suscripción activa</summary>
    [HttpPatch("{id:guid}/max-beneficiaries")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateMaxBeneficiaries(
        Guid id,
        [FromBody] UpdateMaxBeneficiariesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new UpdateMaxBeneficiariesCommand(id, request.MaxBeneficiaries),
            cancellationToken);

        return result.IsFailure ? ToErrorWithCode(result) : NoContent();
    }

    /// <summary>Consultar capacidad y elegibilidad de una suscripción</summary>
    [HttpGet("{id:guid}/capacity")]
    [ProducesResponseType(typeof(SubscriptionCapacityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCapacity(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSubscriptionCapacityQuery(id), cancellationToken);

        return result.IsFailure ? ToErrorWithCode(result) : Ok(result.Value);
    }

    /// <summary>Listar todas las suscripciones (vigentes e históricas) de un cliente titular</summary>
    // La ruta empieza con "/" para ignorar el prefijo del controlador: GET /api/v1/customers/{customerId}/subscriptions
    [HttpGet("/api/v1/customers/{customerId:guid}/subscriptions")]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCustomerSubscriptionsQuery(customerId), cancellationToken);

        return Ok(result.Value);
    }

    // Este módulo además de { message } devuelve el código de error de negocio ({ message, code })
    private IActionResult ToErrorWithCode(Result result) =>
        StatusCode(result.StatusCode, new { message = result.ErrorMessage, code = result.ErrorCode });
}
