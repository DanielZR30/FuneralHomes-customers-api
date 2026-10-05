using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FH.Modules.CustomerPlans.Infrastructure;

public class CustomerPlansDbContextFactory : IDesignTimeDbContextFactory<CustomerPlansDbContext>
{
    public CustomerPlansDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CustomerPlansDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=fh_db_customer;Username=postgres;Password=secret",
            b => b.MigrationsAssembly(typeof(CustomerPlansDbContext).Assembly.FullName));

        return new CustomerPlansDbContext(optionsBuilder.Options);
    }
}
