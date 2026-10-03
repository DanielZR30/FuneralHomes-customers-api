using FH.Modules.CustomerPlans.Application.DTOs;
using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Enums;
using FH.Shared.MockData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FH.Modules.CustomerPlans.Infrastructure.Endpoints;

public static class SubscriptionEndpoints
{
    public static IEndpointRouteBuilder MapSubscriptionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/subscriptions")
            .WithTags("Subscriptions");

        // POST /api/v1/subscriptions - Suscribir cliente a un plan funerario
        group.MapPost("/", (CreateSubscriptionRequest request, InMemoryCustomerStore store) =>
        {
            var customer = store.GetCustomerById(request.CustomerId);
            if (customer is null)
            {
                return Results.NotFound(new { message = $"El cliente con ID {request.CustomerId} no existe." });
            }

            if (customer.Status != CustomerStatus.Active)
            {
                return Results.UnprocessableEntity(new { message = $"El cliente no está activo (Estado: {customer.Status}) y no puede contratar nuevas suscripciones." });
            }

            if (request.MaxBeneficiaries < 1)
            {
                return Results.BadRequest(new { message = "El cupo máximo de beneficiarios debe ser al menos 1." });
            }

            var subscription = CustomerSubscription.Create(
                request.CustomerId,
                request.ExternalPlanId,
                request.MaxBeneficiaries,
                request.StartDate,
                request.EndDate);

            store.AddSubscription(subscription);

            return Results.Created($"/api/v1/subscriptions/{subscription.Id}", SubscriptionResponse.FromEntity(subscription));
        })
        .WithName("CreateSubscription")
        .WithSummary("Suscribir cliente titular a un plan funerario con límite de cupos")
        .Produces<SubscriptionResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        // GET /api/v1/subscriptions/{id} - Obtener suscripción por ID
        group.MapGet("/{id:guid}", (Guid id, InMemoryCustomerStore store) =>
        {
            var subscription = store.GetSubscriptionById(id);

            return subscription is not null
                ? Results.Ok(SubscriptionResponse.FromEntity(subscription))
                : Results.NotFound(new { message = $"Suscripción con ID {id} no encontrada." });
        })
        .WithName("GetSubscriptionById")
        .WithSummary("Consultar detalle de una suscripción funeraria por ID")
        .Produces<SubscriptionResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PATCH /api/v1/subscriptions/{id}/cancel - Cancelar suscripción
        group.MapPatch("/{id:guid}/cancel", (Guid id, InMemoryCustomerStore store) =>
        {
            var subscription = store.GetSubscriptionById(id);
            if (subscription is null)
            {
                return Results.NotFound(new { message = $"Suscripción con ID {id} no encontrada." });
            }

            if (subscription.Status != SubscriptionStatus.Active)
            {
                return Results.BadRequest(new { message = $"La suscripción ya se encuentra en estado {subscription.Status}." });
            }

            subscription.Cancel();

            return Results.Ok(SubscriptionResponse.FromEntity(subscription));
        })
        .WithName("CancelSubscription")
        .WithSummary("Cancelar una suscripción de previsión funeraria")
        .Produces<SubscriptionResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/v1/customers/{customerId}/subscriptions - Suscripciones por cliente
        routes.MapGet("/api/v1/customers/{customerId:guid}/subscriptions", (Guid customerId, InMemoryCustomerStore store) =>
        {
            var subscriptions = store.GetSubscriptionsByCustomer(customerId).Select(SubscriptionResponse.FromEntity);
            return Results.Ok(subscriptions);
        })
        .WithTags("Subscriptions")
        .WithName("GetSubscriptionsByCustomer")
        .WithSummary("Listar todas las suscripciones (vigentes e históricas) de un cliente titular")
        .Produces<IEnumerable<SubscriptionResponse>>(StatusCodes.Status200OK);

        return routes;
    }
}
