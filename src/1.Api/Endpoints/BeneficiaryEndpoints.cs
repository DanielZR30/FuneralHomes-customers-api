using FH.Api.Customer.DTOs;
using FH.Api.Customer.MockData;
using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Enums;

namespace FH.Api.Customer.Endpoints;

public static class BeneficiaryEndpoints
{
    public static IEndpointRouteBuilder MapBeneficiaryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/subscriptions/{subscriptionId:guid}/beneficiaries")
            .WithTags("Beneficiaries");

        // POST /api/v1/subscriptions/{subscriptionId}/beneficiaries - Incorporar nuevo beneficiario (humano o mascota)
        group.MapPost("/", (Guid subscriptionId, AddBeneficiaryRequest request, InMemoryCustomerStore store) =>
        {
            var subscription = store.GetSubscriptionById(subscriptionId);
            if (subscription is null)
            {
                return Results.NotFound(new { message = $"La suscripción {subscriptionId} no existe." });
            }

            if (subscription.Status != SubscriptionStatus.Active)
            {
                return Results.UnprocessableEntity(new { message = $"La suscripción no está activa (Estado: {subscription.Status})." });
            }

            // Validar cupos disponibles en el mock
            var currentActiveCount = store.GetActiveBeneficiariesCount(subscriptionId);

            if (!subscription.CanAcceptBeneficiary(currentActiveCount))
            {
                return Results.Conflict(new { message = $"Se ha alcanzado el cupo máximo de beneficiarios ({subscription.MaxBeneficiaries}) para esta suscripción. Activos: {currentActiveCount}." });
            }

            // Validar coherencia tipo de sujeto y parentesco
            if (request.SubjectType == SubjectType.Pet && request.RelationshipType != RelationshipType.Pet)
            {
                return Results.BadRequest(new { message = "Un sujeto de tipo Mascota (Pet) debe tener relación 'Pet'." });
            }

            // Crear miembro (calcula automáticamente derived_age)
            var member = Member.Create(
                request.SubjectType,
                request.FirstName,
                request.BirthDate,
                request.LastName,
                request.IdentificationType,
                request.IdentificationNumber,
                request.Email,
                request.Phone);

            // Crear vínculo de beneficiario
            var beneficiary = Beneficiary.Create(
                subscriptionId,
                member.Id,
                request.BeneficiaryType,
                request.RelationshipType);

            // Registrar novedad inmutable en la bitácora de auditoría / outbox
            var auditLog = BeneficiaryAuditLog.Create(
                subscriptionId,
                member.Id,
                AuditAction.BeneficiaryAdded,
                eventPublished: false);

            store.AddBeneficiary(member, beneficiary, auditLog);

            var response = BeneficiaryResponse.FromEntity(beneficiary, member);
            return Results.Created($"/api/v1/subscriptions/{subscriptionId}/beneficiaries/{member.Id}", response);
        })
        .WithName("AddBeneficiary")
        .WithSummary("Incorporar un beneficiario (persona o mascota) a una suscripción activa")
        .Produces<BeneficiaryResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        // GET /api/v1/subscriptions/{subscriptionId}/beneficiaries - Listar grupo cubierto de una suscripción
        group.MapGet("/", (Guid subscriptionId, InMemoryCustomerStore store) =>
        {
            var subscription = store.GetSubscriptionById(subscriptionId);
            if (subscription is null)
            {
                return Results.NotFound(new { message = $"La suscripción {subscriptionId} no existe." });
            }

            var beneficiaries = store.GetBeneficiariesBySubscription(subscriptionId);
            var response = beneficiaries.Select(item => BeneficiaryResponse.FromEntity(item.Beneficiary, item.Member));

            return Results.Ok(response);
        })
        .WithName("GetBeneficiariesBySubscription")
        .WithSummary("Listar todos los beneficiarios vinculados a una suscripción (con edad derivada y parentesco)")
        .Produces<IEnumerable<BeneficiaryResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/subscriptions/{subscriptionId}/beneficiaries/{memberId} - Retirar beneficiario
        group.MapDelete("/{memberId:guid}", (Guid subscriptionId, Guid memberId, InMemoryCustomerStore store) =>
        {
            var beneficiary = store.GetBeneficiary(subscriptionId, memberId);

            if (beneficiary is null)
            {
                return Results.NotFound(new { message = $"El beneficiario no se encuentra vinculado a esta suscripción." });
            }

            if (beneficiary.Status == BeneficiaryStatus.Removed)
            {
                return Results.BadRequest(new { message = "El beneficiario ya había sido retirado previamente." });
            }

            beneficiary.Remove();

            // Auditoría / Outbox
            var auditLog = BeneficiaryAuditLog.Create(
                subscriptionId,
                memberId,
                AuditAction.BeneficiaryRemoved,
                eventPublished: false);

            store.AddAuditLog(auditLog);

            return Results.NoContent();
        })
        .WithName("RemoveBeneficiary")
        .WithSummary("Desafiliar/retirar a un beneficiario de una suscripción funeraria")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return routes;
    }
}
