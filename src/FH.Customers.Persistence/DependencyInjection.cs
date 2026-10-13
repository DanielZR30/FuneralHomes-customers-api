using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FH.Customers.Application.UseCases.CustomerPlans;
using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Persistence.Repositories;
using FH.Customers.Application.Contracts.Persistence;
using FH.Customers.Persistence.Seeds;
using FH.Customers.Persistence.UnitOfWorks;

namespace FH.Customers.Persistence;

public static class DependencyInjection
{
    /// <summary>
    /// Registra la capa Persistence: DbContext de EF Core (PostgreSQL), repositorios y Unit of Work.
    /// </summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        var connectionString = configuration?.GetConnectionString("CustomerDb")
            ?? "Host=localhost;Port=5432;Database=fh_db_customer;Username=postgres;Password=secret";

        // Las migraciones del DbContext viven en este mismo ensamblado (FH.Customers.Persistence)
        var migrationsAssembly = typeof(DependencyInjection).Assembly.FullName;

        services.AddDbContext<CustomerDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(migrationsAssembly)));

        // Repository genérico + Unit of Work
        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<CustomerDbContext>());

        // Repositorios por módulo
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        services.AddScoped<IBeneficiaryAuditLogRepository, BeneficiaryAuditLogRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        // Seeder para desarrollo
        services.AddScoped<IDataSeeder, SubscriptionSeeder>();

        return services;
    }
}
