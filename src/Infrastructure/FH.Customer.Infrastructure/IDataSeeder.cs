namespace FH.Modules.CustomerPlans.Infrastructure;

public interface IDataSeeder
{
    int Order { get; }
    Task SeedAsync(CancellationToken cancellationToken = default);
}
