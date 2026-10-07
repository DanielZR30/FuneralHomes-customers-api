using Microsoft.EntityFrameworkCore;
using FH.Customers.Domain.Audit.Repositories;
using FH.Customers.Domain.Entities;
using FH.Customers.Persistence;

namespace FH.Customers.Persistence.Audit.Repositories;

public class BeneficiaryAuditLogRepository : IBeneficiaryAuditLogRepository
{
    private readonly CustomerDbContext _context;

    public BeneficiaryAuditLogRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(BeneficiaryAuditLog log, CancellationToken cancellationToken = default)
    {
        await _context.BeneficiaryAuditLogs.AddAsync(log, cancellationToken);
    }

    public async Task<BeneficiaryAuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BeneficiaryAuditLogs
            .FirstOrDefaultAsync(log => log.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<BeneficiaryAuditLog>> GetBySubscriptionAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.BeneficiaryAuditLogs
            .Where(log => log.SubscriptionId == subscriptionId)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BeneficiaryAuditLog>> GetByMemberAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        return await _context.BeneficiaryAuditLogs
            .Where(log => log.MemberId == memberId)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BeneficiaryAuditLog>> GetUnpublishedAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.BeneficiaryAuditLogs
            .Where(log => !log.EventPublished)
            .OrderBy(log => log.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }
}
