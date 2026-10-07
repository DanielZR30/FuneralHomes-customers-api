using Microsoft.EntityFrameworkCore;
using FH.Customers.Application.Events;
using FH.Customers.Domain.Common;
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.Entities;
using FH.Customers.Persistence.Configurations;
using FH.Customers.Persistence.CustomerPlans;
using FH.Customers.Persistence.CustomerPlans.Configurations;

namespace FH.Customers.Persistence;

/// <summary>
/// Único DbContext del microservicio Customers (una sola base de datos fh_db_customer).
/// </summary>
public class CustomerDbContext : DbContext
{
    private readonly IDomainEventDispatcher? _dispatcher;

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<BeneficiaryAuditLog> BeneficiaryAuditLogs => Set<BeneficiaryAuditLog>();

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options, IDomainEventDispatcher? dispatcher = null) : base(options)
    {
        _dispatcher = dispatcher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new MemberConfiguration());
        modelBuilder.ApplyConfiguration(new BeneficiaryConfiguration());
        modelBuilder.ApplyConfiguration(new BeneficiaryAuditLogConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        if (_dispatcher is null)
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

        var events = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in events)
        {
            await _dispatcher.DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
