using FH.Modules.Audit.Domain.Entities;

namespace FH.Modules.Audit.Domain.Repositories;

public interface IBeneficiaryAuditLogRepository
{
    Task AddAsync(BeneficiaryAuditLog log, CancellationToken cancellationToken = default);

    Task<BeneficiaryAuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BeneficiaryAuditLog>> GetBySubscriptionAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BeneficiaryAuditLog>> GetByMemberAsync(
        Guid memberId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BeneficiaryAuditLog>> GetUnpublishedAsync(
        int batchSize,
        CancellationToken cancellationToken = default);
}
