using FH.Shared.Domain.Entities;
using FH.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FH.Shared.Persistence;

public class CustomerDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerSubscription> CustomerSubscriptions => Set<CustomerSubscription>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<BeneficiaryAuditLog> BeneficiaryAuditLogs => Set<BeneficiaryAuditLog>();

    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Registro explícito y por ensamblado de las configuraciones de entidades
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerSubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new MemberConfiguration());
        modelBuilder.ApplyConfiguration(new BeneficiaryConfiguration());
        modelBuilder.ApplyConfiguration(new BeneficiaryAuditLogConfiguration());
    }
}
