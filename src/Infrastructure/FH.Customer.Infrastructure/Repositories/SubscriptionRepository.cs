using FH.Modules.CustomerPlans.Application.Abstractions;
using FH.Modules.CustomerPlans.Domain;
using FH.Shared.Domain.Enums;
using FH.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FH.Customer.Api.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly CustomerDbContext _context;

    public SubscriptionRepository(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.Period.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetActiveByCustomerAndPlanAsync(
        Guid customerId,
        Guid externalPlanId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.CustomerId == customerId &&
                        s.ExternalPlanId == externalPlanId &&
                        s.Status == SubscriptionStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        await _context.Subscriptions.AddAsync(subscription, cancellationToken);
    }

    public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _context.Subscriptions.Update(subscription);
        return Task.CompletedTask;
    }
}
