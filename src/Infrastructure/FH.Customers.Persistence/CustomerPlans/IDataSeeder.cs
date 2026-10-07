namespace FH.Customers.Persistence.CustomerPlans;

public interface IDataSeeder
{
    int Order { get; }
    Task SeedAsync(CancellationToken cancellationToken = default);
}
