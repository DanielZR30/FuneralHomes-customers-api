using FH.Api.Customer.DTOs;
using FH.Api.Customer.MockData;
using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FH.Api.Customer.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/customers")
            .WithTags("Customers");

        // POST /api/v1/customers - Crear un nuevo cliente titular (B2C o B2B)
        group.MapPost("/", (CreateCustomerRequest request, InMemoryCustomerStore store) =>
        {
            var normalizedDocType = request.IdentificationType.Trim().ToUpperInvariant();
            var normalizedDocNum = request.IdentificationNumber.Trim().ToUpperInvariant();

            if (store.ExistsCustomerIdentification(normalizedDocType, normalizedDocNum))
            {
                return Results.Conflict(new { message = $"Ya existe un cliente registrado con identificación {normalizedDocType} {normalizedDocNum}." });
            }

            var customer = FH.Shared.Domain.Entities.Customer.Create(
                request.CustomerType,
                request.Name,
                normalizedDocType,
                normalizedDocNum,
                request.Email,
                request.Phone,
                request.Address);

            store.AddCustomer(customer);

            return Results.Created($"/api/v1/customers/{customer.Id}", CustomerResponse.FromEntity(customer));
        })
        .WithName("CreateCustomer")
        .WithSummary("Registrar un nuevo cliente titular (Persona Natural o Empresa)")
        .Produces<CustomerResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // GET /api/v1/customers - Listar clientes
        group.MapGet("/", (InMemoryCustomerStore store) =>
        {
            var customers = store.GetCustomers().Select(CustomerResponse.FromEntity);
            return Results.Ok(customers);
        })
        .WithName("GetCustomers")
        .WithSummary("Listar clientes registrados (Mock en Memoria)")
        .Produces<IEnumerable<CustomerResponse>>(StatusCodes.Status200OK);

        // GET /api/v1/customers/{id} - Obtener cliente por ID
        group.MapGet("/{id:guid}", (Guid id, InMemoryCustomerStore store) =>
        {
            var customer = store.GetCustomerById(id);

            return customer is not null
                ? Results.Ok(CustomerResponse.FromEntity(customer))
                : Results.NotFound(new { message = $"Cliente con ID {id} no fue encontrado." });
        })
        .WithName("GetCustomerById")
        .WithSummary("Consultar ficha de cliente por UUID")
        .Produces<CustomerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/v1/customers/by-identification - Buscar cliente por documento
        group.MapGet("/by-identification", ([FromQuery] string type, [FromQuery] string number, InMemoryCustomerStore store) =>
        {
            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(number))
            {
                return Results.BadRequest(new { message = "El tipo y número de identificación son obligatorios." });
            }

            var customer = store.GetCustomerByIdentification(type, number);

            return customer is not null
                ? Results.Ok(CustomerResponse.FromEntity(customer))
                : Results.NotFound(new { message = $"No se encontró cliente con {type.ToUpperInvariant()} {number.ToUpperInvariant()}." });
        })
        .WithName("GetCustomerByIdentification")
        .WithSummary("Consultar cliente por tipo y número de documento")
        .Produces<CustomerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PUT /api/v1/customers/{id} - Actualizar datos de contacto y demográficos
        group.MapPut("/{id:guid}", (Guid id, UpdateCustomerRequest request, InMemoryCustomerStore store) =>
        {
            var customer = store.GetCustomerById(id);
            if (customer is null)
            {
                return Results.NotFound(new { message = $"Cliente con ID {id} no fue encontrado." });
            }

            customer.UpdateDemographics(request.Name, request.Email, request.Phone, request.Address);

            return Results.Ok(CustomerResponse.FromEntity(customer));
        })
        .WithName("UpdateCustomerDemographics")
        .WithSummary("Actualizar datos demográficos y de contacto de un cliente")
        .Produces<CustomerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PATCH /api/v1/customers/{id}/status - Cambiar estado (Active, Inactive, Suspended)
        group.MapPatch("/{id:guid}/status", (Guid id, ChangeCustomerStatusRequest request, InMemoryCustomerStore store) =>
        {
            var customer = store.GetCustomerById(id);
            if (customer is null)
            {
                return Results.NotFound(new { message = $"Cliente con ID {id} no fue encontrado." });
            }

            switch (request.Status)
            {
                case CustomerStatus.Active:
                    customer.Activate();
                    break;
                case CustomerStatus.Inactive:
                    customer.Deactivate();
                    break;
                case CustomerStatus.Suspended:
                    customer.Suspend();
                    break;
            }

            return Results.NoContent();
        })
        .WithName("ChangeCustomerStatus")
        .WithSummary("Modificar estado de un cliente (Activar, Suspender, Inactivar)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return routes;
    }
}
