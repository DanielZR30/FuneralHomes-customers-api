using FH.Modules.CustomerPlans.Domain;
using FH.Modules.CustomerPlans.Infrastructure.Configurations;
using FH.Shared.Domain.Common;
using FH.Shared.Domain.Entities;
using FH.Shared.Infrastructure.Persistence.Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FH.Shared.Persistence;

public class CustomerDbContext : DbContext
{
    private readonly IPublisher? _publisher;

    public DbSet<FH.Shared.Domain.Entities.Customer> Customers => Set<FH.Shared.Domain.Entities.Customer>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<BeneficiaryAuditLog> BeneficiaryAuditLogs => Set<BeneficiaryAuditLog>();

    public CustomerDbContext(
        DbContextOptions<CustomerDbContext> options,
        IPublisher? publisher = null) : base(options)
    {
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Registro explícito de las configuraciones de entidades de negocio
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
