
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Domain.Beneficiaries.Repositories;

public interface IBeneficiaryRepository
{
    Task<IReadOnlyList<Beneficiary>> GetBySubscriptionAsync(
        Guid subscriptionId,
        BeneficiaryStatus? status,
        CancellationToken cancellationToken = default);

    Task<Beneficiary?> GetAsync(
        Guid subscriptionId,
        Guid memberId,
        CancellationToken cancellationToken = default);

    Task<int> CountActiveAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Member member,
        Beneficiary beneficiary,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByIdentificationAsync(
        Guid subscriptionId,
        string identificationType,
        string identificationNumber,
        CancellationToken cancellationToken = default,
        Guid? excludeMemberId = null);
}