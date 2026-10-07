using FH.Modules.Customer.Application.Commands.ChangeCustomerStatus;
using FH.Modules.Customer.Application.Commands.CreateCustomer;
using FH.Modules.Customer.Application.Commands.UpdateCustomerDemographics;
using FH.Modules.Customer.Application.DTOs;
using FH.Modules.Customer.Application.Queries.GetCustomerById;
using FH.Modules.Customer.Application.Queries.GetCustomerByIdentification;
using FH.Modules.Customer.Application.Queries.GetCustomers;
using FH.Shared.Application.Common;
using FH.Shared.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FH.Customer.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] CustomerStatus? status,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomersQuery(status);
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        return result.IsFailure ? ToActionResult(result) : Ok(result.Value);
    }

    [HttpGet("by-identification")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdentification(
        [FromQuery] string type,
        [FromQuery] string number,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdentificationQuery(type, number);
        var result = await _sender.Send(query, cancellationToken);

        return result.IsFailure ? ToActionResult(result) : Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.CustomerType,
            request.Name,
            request.IdentificationType,
            request.IdentificationNumber,
            request.Email,
            request.Phone,
            request.Address);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}/demographics")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDemographics(
        Guid id,
        [FromBody] UpdateCustomerDemographicsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerDemographicsCommand(
            id,
            request.Name,
            request.Email,
            request.Phone,
            request.Address);

        var result = await _sender.Send(command, cancellationToken);

        return result.IsFailure ? ToActionResult(result) : Ok(result.Value);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeCustomerStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeCustomerStatusCommand(id, request.NewStatus);
        var result = await _sender.Send(command, cancellationToken);

        return result.IsFailure ? ToActionResult(result) : NoContent();
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
