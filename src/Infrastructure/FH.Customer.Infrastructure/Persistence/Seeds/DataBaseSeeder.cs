using FH.Modules.CustomerPlans.Infrastructure;
using FH.Shared.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FH.Customer.Infrastructure.Persistence.Seeds;

public static class DataBaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetService<ILogger<CustomerDbContext>>();

        try
        {
            var context = services.GetService<CustomerDbContext>();
            if (context is not null)
            {
                var creator = context.Database.GetService<IRelationalDatabaseCreator>();
                try
                {
                    await creator.CreateTablesAsync();
                }
                catch
                {
                    // Tablas ya creadas
                }
            }

            var seeders = services.GetServices<IDataSeeder>().OrderBy(s => s.Order);
            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync();
            }

            logger?.LogInformation("DataBaseSeeder: Inicialización de tablas y seeders completada con éxito.");
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "DataBaseSeeder: No se pudo conectar a la base de datos.");
        }
    }
}
