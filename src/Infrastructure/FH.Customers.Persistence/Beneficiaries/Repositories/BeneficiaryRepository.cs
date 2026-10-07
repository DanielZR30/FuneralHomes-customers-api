using Microsoft.EntityFrameworkCore;
using FH.Customers.Domain.Beneficiaries.Repositories;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;
using FH.Customers.Persistence;

namespace FH.Customers.Persistence.Beneficiaries.Repositories;

public class BeneficiaryRepository : IBeneficiaryRepository
{
    private readonly CustomerDbContext _context;

    public BeneficiaryRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Beneficiary>> GetBySubscriptionAsync(
        Guid subscriptionId,
        BeneficiaryStatus? status,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Beneficiary> query = _context.Beneficiaries
            .Include(b => b.Member)
            .Where(b => b.SubscriptionId == subscriptionId);

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        return await query
            .OrderBy(b => b.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsActiveByIdentificationAsync(
        Guid subscriptionId,
        string identificationType,
        string identificationNumber,
        CancellationToken cancellationToken = default,
        Guid? excludeMemberId = null)
    {
        return await _context.Beneficiaries
            .AnyAsync(
                b => b.SubscriptionId == subscriptionId
                     && b.Status == BeneficiaryStatus.Active
                     && (excludeMemberId == null || b.MemberId != excludeMemberId)
                     && b.Member!.Document!.Type == identificationType
                     && b.Member.Document!.Number == identificationNumber,
                cancellationToken);
    }

    public async Task<Beneficiary?> GetAsync(
        Guid subscriptionId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Beneficiaries
            .Include(b => b.Member)
            .FirstOrDefaultAsync(
                b => b.SubscriptionId == subscriptionId && b.MemberId == memberId,
                cancellationToken);
    }

    public async Task<int> CountActiveAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Beneficiaries
            .CountAsync(
                b => b.SubscriptionId == subscriptionId && b.Status == BeneficiaryStatus.Active,
                cancellationToken);
    }

    public async Task AddAsync(
        Member member,
        Beneficiary beneficiary,
        CancellationToken cancellationToken = default)
    {
        await _context.Members.AddAsync(member, cancellationToken);
        await _context.Beneficiaries.AddAsync(beneficiary, cancellationToken);
    }
}