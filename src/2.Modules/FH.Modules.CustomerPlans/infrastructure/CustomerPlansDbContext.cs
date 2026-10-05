using FH.Modules.CustomerPlans.Domain;
using FH.Modules.CustomerPlans.Infrastructure.Configurations;
using FH.Shared.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FH.Modules.CustomerPlans.Infrastructure;

public class CustomerPlansDbContext : DbContext
{
    private readonly IPublisher? _publisher;

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public CustomerPlansDbContext(
        DbContextOptions<CustomerPlansDbContext> options,
        IPublisher? publisher = null) : base(options)
    {
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        if (_publisher is null)
        {
            return;
        }

        var entitiesWithEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        if (entitiesWithEvents.Count == 0)
        {
            return;
        }

        var events = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in events)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
