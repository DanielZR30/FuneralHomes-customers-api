using Microsoft.Extensions.DependencyInjection;

namespace FH.Customers.Persistence.Seeds;

/// <summary>
/// Ejecuta todos los seeders registrados (IDataSeeder) en el orden indicado por su propiedad Order.
/// Se invoca una sola vez al arrancar la aplicación.
/// </summary>
public static class DataBaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        var seeders = scope.ServiceProvider
            .GetServices<IDataSeeder>()
            .OrderBy(seeder => seeder.Order);

        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync(cancellationToken);
        }
    }
}
