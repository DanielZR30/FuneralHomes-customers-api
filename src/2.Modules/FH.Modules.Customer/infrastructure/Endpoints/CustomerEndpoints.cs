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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FH.Modules.Customer.Infrastructure.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/customers")
            .WithTags("Customers");

        // POST /api/v1/customers - Crear un nuevo cliente titular (B2C o B2B)
        group.MapPost("/", async (
            CreateCustomerRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateCustomerCommand(
                request.CustomerType,
                request.Name,
                request.IdentificationType,
                request.IdentificationNumber,
                request.Email,
                request.Phone,
                request.Address);

            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return ToError(result);
            }

            return Results.Created($"/api/v1/customers/{result.Value.Id}", result.Value);
        })
        .WithName("CreateCustomer")
        .WithSummary("Registrar un nuevo cliente titular (Persona Natural o Empresa)")
        .Produces<CustomerResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // GET /api/v1/customers - Listar clientes (filtro opcional por estado)
        group.MapGet("/", async (
            [FromQuery] CustomerStatus? status,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCustomersQuery(status), cancellationToken);

            return result.IsFailure ? ToError(result) : Results.Ok(result.Value);
        })
        .WithName("GetCustomers")
        .WithSummary("Listar clientes registrados, con filtro opcional por estado")
        .Produces<IReadOnlyList<CustomerResponse>>(StatusCodes.Status200OK);

        // GET /api/v1/customers/by-identification - Buscar cliente por documento
        group.MapGet("/by-identification", async (
            [FromQuery] string type,
            [FromQuery] string number,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetCustomerByIdentificationQuery(type, number),
                cancellationToken);

            return result.IsFailure ? ToError(result) : Results.Ok(result.Value);
        })
        .WithName("GetCustomerByIdentification")
        .WithSummary("Consultar cliente por tipo y número de documento")
        .Produces<CustomerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/v1/customers/{id} - Obtener cliente por ID
        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCustomerByIdQuery(id), cancellationToken);

            return result.IsFailure ? ToError(result) : Results.Ok(result.Value);
        })
        .WithName("GetCustomerById")
        .WithSummary("Consultar ficha de cliente por UUID")
        .Produces<CustomerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PUT /api/v1/customers/{id} - Actualizar datos de contacto y demográficos
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateCustomerRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateCustomerDemographicsCommand(
                id,
                request.Name,
                request.Email,
                request.Phone,
                request.Address);

            var result = await sender.Send(command, cancellationToken);

            return result.IsFailure ? ToError(result) : Results.Ok(result.Value);
        })
        .WithName("UpdateCustomerDemographics")
        .WithSummary("Actualizar datos demográficos y de contacto (el documento de identificación es inmutable)")
        .Produces<CustomerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PATCH /api/v1/customers/{id}/status - Cambiar estado (Active, Inactive, Suspended)
        group.MapPatch("/{id:guid}/status", async (
            Guid id,
            ChangeCustomerStatusRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new ChangeCustomerStatusCommand(id, request.Status),
                cancellationToken);

            return result.IsFailure ? ToError(result) : Results.NoContent();
        })
        .WithName("ChangeCustomerStatus")
        .WithSummary("Modificar estado de un cliente (Activar, Suspender, Inactivar)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return routes;
    }

    // Traduce un Result fallido a la respuesta HTTP con el código que trae (400, 404, 409, 422...)
    private static IResult ToError(Result result) =>
        Results.Json(new { message = result.ErrorMessage }, statusCode: result.StatusCode);
}
