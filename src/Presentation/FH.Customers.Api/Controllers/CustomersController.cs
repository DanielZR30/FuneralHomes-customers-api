using Microsoft.AspNetCore.Mvc;
using FH.Customers.Application.Customers.Commands.ChangeCustomerStatus;
using FH.Customers.Application.Customers.Commands.CreateCustomer;
using FH.Customers.Application.Customers.Commands.UpdateCustomerDemographics;
using FH.Customers.Application.Customers.DTOs;
using FH.Customers.Application.Customers.Queries.GetCustomerById;
using FH.Customers.Application.Customers.Queries.GetCustomerByIdentification;
using FH.Customers.Application.Customers.Queries.GetCustomers;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Api.Controllers;

[Route("api/v1/customers")]
[Tags("Customers")]
public class CustomersController : ApiControllerBase
{
    /// <summary>Registrar un nuevo cliente titular (Persona Natural o Empresa)</summary>
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

        var result = await Mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return ToError(result);
        }

        return Created($"/api/v1/customers/{result.Value.Id}", result.Value);
    }

    /// <summary>Listar clientes registrados, con filtro opcional por estado</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] CustomerStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCustomersQuery(status), cancellationToken);

        return result.IsFailure ? ToError(result) : Ok(result.Value);
    }

    /// <summary>Consultar cliente por tipo y número de documento</summary>
    [HttpGet("by-identification")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdentification(
        [FromQuery] string type,
        [FromQuery] string number,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetCustomerByIdentificationQuery(type, number),
            cancellationToken);

        return result.IsFailure ? ToError(result) : Ok(result.Value);
    }

    /// <summary>Consultar ficha de cliente por UUID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCustomerByIdQuery(id), cancellationToken);

        return result.IsFailure ? ToError(result) : Ok(result.Value);
    }

    /// <summary>Actualizar datos demográficos y de contacto (el documento de identificación es inmutable)</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerDemographicsCommand(
            id,
            request.Name,
            request.Email,
            request.Phone,
            request.Address);

        var result = await Mediator.Send(command, cancellationToken);

        return result.IsFailure ? ToError(result) : Ok(result.Value);
    }

    /// <summary>Modificar estado de un cliente (Activar, Suspender, Inactivar)</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeCustomerStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new ChangeCustomerStatusCommand(id, request.Status),
            cancellationToken);

        return result.IsFailure ? ToError(result) : NoContent();
    }
}
