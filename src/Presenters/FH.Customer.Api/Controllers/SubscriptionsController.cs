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

namespace FH.Customer.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISender _sender;

    public SubscriptionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Subscribe(
        [FromBody] SubscribeCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSubscriptionByIdQuery(id), cancellationToken);
        return result.IsFailure ? ToActionResult(result) : Ok(result.Value);
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelSubscriptionCommand(id), cancellationToken);
        return result.IsFailure ? ToActionResult(result) : Ok(result.Value);
    }

    [HttpPatch("{id:guid}/max-beneficiaries")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateMaxBeneficiaries(
        Guid id,
        [FromBody] UpdateMaxBeneficiariesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateMaxBeneficiariesCommand(id, request.NewMaxBeneficiaries), cancellationToken);
        return result.IsFailure ? ToActionResult(result) : Ok(result.Value);
    }

    [HttpGet("{id:guid}/capacity")]
    [ProducesResponseType(typeof(SubscriptionCapacityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCapacity(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSubscriptionCapacityQuery(id), cancellationToken);
        return result.IsFailure ? ToActionResult(result) : Ok(result.Value);
    }

    [HttpGet("/api/v1/customers/{customerId:guid}/subscriptions")]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCustomerSubscriptionsQuery(customerId), cancellationToken);
        return Ok(result.Value);
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

public record CancelSubscriptionRequest(string? Reason);
public record UpdateMaxBeneficiariesRequest(int NewMaxBeneficiaries);
