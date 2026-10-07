using CustomerEntity = FH.Modules.Customer.Domain.Entities.Customer;
using FH.Modules.Customer.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FH.Modules.Customer.Infrastructure.Persistence;

public class CustomerModuleDbContext : DbContext
{
    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();

    public CustomerModuleDbContext(DbContextOptions<CustomerModuleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
    }
}
