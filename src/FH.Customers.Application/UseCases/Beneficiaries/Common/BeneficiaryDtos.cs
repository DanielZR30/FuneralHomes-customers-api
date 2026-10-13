using FH.Customers.Domain.Common.Enums;
using FH.Customers.Domain.Entities;

namespace FH.Customers.Application.UseCases.Beneficiaries.Common;

public record BeneficiaryResponse(
    Guid Id,
    Guid SubscriptionId,
    Guid MemberId,
    string SubjectType,
    string FullName,
    string? IdentificationType,
    string? IdentificationNumber,
    DateOnly BirthDate,
    int DerivedAge,
    string BeneficiaryType,
    string RelationshipType,
    string Status,
    DateTime JoinedAt,
    DateTime? RemovedAt)
{
    public static BeneficiaryResponse FromEntity(Beneficiary beneficiary, Member member) =>
        new(
            beneficiary.Id,
            beneficiary.SubscriptionId,
            beneficiary.MemberId,
            member.SubjectType.ToString(),
            string.IsNullOrWhiteSpace(member.LastName) ? member.FirstName : $"{member.FirstName} {member.LastName}",
            member.IdentificationType,
            member.IdentificationNumber,
            member.BirthDate,
            member.DerivedAge,
            beneficiary.BeneficiaryType.ToString(),
            beneficiary.RelationshipType.ToString(),
            beneficiary.Status.ToString(),
            beneficiary.JoinedAt,
            beneficiary.RemovedAt);

}
