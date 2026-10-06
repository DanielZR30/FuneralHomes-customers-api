using FH.Modules.CustomerPlans.Application.Abstractions;
using FH.Modules.CustomerPlans.Infrastructure.Gateways;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Modules.CustomerPlans.Infrastructure;

public static class CustomerPlansPersistenceServices
{
    public static IServiceCollection AddCustomerPlansPersistenceServices(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        var connectionString = configuration?.GetConnectionString("CustomerDb")
            ?? "Host=localhost;Port=5432;Database=fh_db_customer;Username=postgres;Password=secret";

        services.AddDbContext<CustomerPlansDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(CustomerPlansDbContext).Assembly.FullName);
            });
        });

        // Repositorio y Unit of Work
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ICustomerPlansUnitOfWork, CustomerPlansUnitOfWork>();

        // Gateways y adaptadores de puertos
        services.AddScoped<ICustomerExistenceChecker, CustomerExistenceChecker>();
        services.AddScoped<IFinancialsPlanGateway, FinancialsPlanGatewayStub>();
        services.AddScoped<IAssignedBeneficiariesCounter, AssignedBeneficiariesCounterStub>();

        // Seeder para desarrollo
        services.AddScoped<IDataSeeder, SubscriptionSeeder>();

        return services;
    }
}
