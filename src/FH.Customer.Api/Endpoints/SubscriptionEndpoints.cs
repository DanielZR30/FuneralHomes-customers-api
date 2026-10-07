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
        group.MapPost("/", async (
            CreateSubscriptionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new SubscribeCustomerCommand(
                request.CustomerId,
                request.ExternalPlanId,
                request.MaxBeneficiaries,
                request.StartDate,
                request.EndDate);

            var result = await sender.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return ToError(result);
            }

            return Results.Created($"/api/v1/subscriptions/{result.Value.Id}", result.Value);
        })
        .WithName("CreateSubscriptionEndpoint")
        .WithSummary("Suscribir cliente titular a un plan funerario con límite de cupos")
        .Produces<SubscriptionDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        // GET /api/v1/subscriptions/{id} - Obtener suscripción por ID
        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSubscriptionByIdQuery(id);
            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? ToError(result) : Results.Ok(result.Value);
        })
        .WithName("GetSubscriptionByIdEndpoint")
        .WithSummary("Consultar detalle de una suscripción funeraria por ID")
        .Produces<SubscriptionDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PATCH /api/v1/subscriptions/{id}/cancel - Cancelar suscripción
        group.MapPatch("/{id:guid}/cancel", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CancelSubscriptionCommand(id);
            var result = await sender.Send(command, cancellationToken);

            return result.IsFailure ? ToError(result) : Results.NoContent();
        })
        .WithName("CancelSubscriptionEndpoint")
        .WithSummary("Cancelar una suscripción de previsión funeraria")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        // PATCH /api/v1/subscriptions/{id}/max-beneficiaries - Modificar cupo
        group.MapPatch("/{id:guid}/max-beneficiaries", async (
            Guid id,
            UpdateMaxBeneficiariesRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateMaxBeneficiariesCommand(id, request.MaxBeneficiaries);
            var result = await sender.Send(command, cancellationToken);

            return result.IsFailure ? ToError(result) : Results.NoContent();
        })
        .WithName("UpdateMaxBeneficiariesEndpoint")
        .WithSummary("Modificar el cupo máximo de beneficiarios de una suscripción activa")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        // GET /api/v1/subscriptions/{id}/capacity - Consultar cupo y elegibilidad
        group.MapGet("/{id:guid}/capacity", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSubscriptionCapacityQuery(id);
            var result = await sender.Send(query, cancellationToken);

            return result.IsFailure ? ToError(result) : Results.Ok(result.Value);
        })
        .WithName("GetSubscriptionCapacityEndpoint")
        .WithSummary("Consultar capacidad y elegibilidad de una suscripción")
        .Produces<SubscriptionCapacityDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/v1/customers/{customerId}/subscriptions - Suscripciones por cliente
        routes.MapGet("/api/v1/customers/{customerId:guid}/subscriptions", async (
            Guid customerId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCustomerSubscriptionsQuery(customerId);
            var result = await sender.Send(query, cancellationToken);

            return Results.Ok(result.Value);
        })
        .WithTags("Subscriptions")
        .WithName("GetSubscriptionsByCustomerEndpoint")
        .WithSummary("Listar todas las suscripciones (vigentes e históricas) de un cliente titular")
        .Produces<IReadOnlyList<SubscriptionDto>>(StatusCodes.Status200OK);

        return routes;
    }

    private static IResult ToError(Result result) =>
        Results.Json(new { message = result.ErrorMessage, code = result.ErrorCode }, statusCode: result.StatusCode);
}
