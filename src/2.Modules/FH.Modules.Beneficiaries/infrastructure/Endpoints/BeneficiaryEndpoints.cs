using FH.Modules.Beneficiaries.Application.Commands.AddBeneficiary;
using FH.Modules.Beneficiaries.Application.Commands.RemoveBeneficiary;
using FH.Modules.Beneficiaries.Application.DTOs;
using FH.Modules.Beneficiaries.Application.Queries.GetBeneficiariesBySubscription;
using FH.Shared.Application.Common;
using FH.Shared.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FH.Modules.Beneficiaries.Infrastructure.Endpoints;

public static class BeneficiaryEndpoints
{
    public static IEndpointRouteBuilder MapBeneficiaryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/subscriptions/{subscriptionId:guid}/beneficiaries")
            .WithTags("Beneficiaries");

        // POST /api/v1/subscriptions/{subscriptionId}/beneficiaries - Incorporar beneficiario (humano o mascota)
        group.MapPost("/", async (
            Guid subscriptionId,
            AddBeneficiaryRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
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

            var result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                return ToError(result);
            }

            return Results.Created(
                $"/api/v1/subscriptions/{subscriptionId}/beneficiaries/{result.Value.MemberId}",
                result.Value);
        })
        .WithName("AddBeneficiary")
        .WithSummary("Incorporar un beneficiario (persona o mascota) a una suscripción activa")
        .Produces<BeneficiaryResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        // GET /api/v1/subscriptions/{subscriptionId}/beneficiaries - Listar grupo cubierto (filtro opcional por estado)
        group.MapGet("/", async (
            Guid subscriptionId,
            [FromQuery] BeneficiaryStatus? status,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetBeneficiariesBySubscriptionQuery(subscriptionId, status),
                cancellationToken);

            return result.IsFailure ? ToError(result) : Results.Ok(result.Value);
        })
        .WithName("GetBeneficiariesBySubscription")
        .WithSummary("Listar los beneficiarios de una suscripción (con edad derivada y parentesco)")
        .Produces<IReadOnlyList<BeneficiaryResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/subscriptions/{subscriptionId}/beneficiaries/{memberId} - Retirar beneficiario
        group.MapDelete("/{memberId:guid}", async (
            Guid subscriptionId,
            Guid memberId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new RemoveBeneficiaryCommand(subscriptionId, memberId),
                cancellationToken);

            return result.IsFailure ? ToError(result) : Results.NoContent();
        })
        .WithName("RemoveBeneficiary")
        .WithSummary("Desafiliar a un beneficiario de una suscripción funeraria")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return routes;
    }

    // Traduce un Result fallido a la respuesta HTTP con el código que trae (400, 404, 409, 422...)
    private static IResult ToError(Result result) =>
        Results.Json(new { message = result.ErrorMessage }, statusCode: result.StatusCode);
}