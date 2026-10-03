using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Enums;

namespace FH.Api.Customer.DTOs;

public record AddBeneficiaryRequest(
    SubjectType SubjectType,
    string FirstName,
    DateOnly BirthDate,
    BeneficiaryType BeneficiaryType,
    RelationshipType RelationshipType,
    string? LastName = null,
    string? IdentificationType = null,
    string? IdentificationNumber = null,
    string? Email = null,
    string? Phone = null);

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
