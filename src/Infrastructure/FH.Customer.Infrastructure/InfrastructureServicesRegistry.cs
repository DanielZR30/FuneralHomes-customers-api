using FH.Customer.Api.Infrastructure;
using FH.Customer.Api.Infrastructure.Repositories;
using FH.Customer.Infrastructure.Messaging;
using FH.Customer.Infrastructure.Persistence.Seeds;
using FH.Messaging.Application;
using FH.Modules.Audit.Domain.Repositories;
using FH.Modules.Audit.Infrastructure.Repositories;
using FH.Modules.Beneficiaries.Domain.Repositories;
using FH.Modules.Beneficiaries.Infrastructure.Repositories;
using FH.Modules.Customer.Domain.Repositories;
using FH.Modules.Customer.Infrastructure.Repositories;
using FH.Modules.CustomerPlans.Application.Abstractions;
using FH.Modules.CustomerPlans.Infrastructure;
using FH.Modules.CustomerPlans.Infrastructure.Gateways;
using FH.Shared.Domain.Repositories;
using FH.Shared.Infrastructure.Persistence.Repositories;
using FH.Shared.MockData;
using FH.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Customer.Infrastructure;

public static class InfrastructureServicesRegistry
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CustomerDb")
            ?? "Host=localhost;Port=5432;Database=fh_db_customer;Username=postgres;Password=secret";

        // DbContext
        services.AddDbContext<CustomerDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        // Repositorios genéricos y Unit of Work
        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<CustomerDbContext>());

        // Repositorios específicos
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ICustomerPlansUnitOfWork, CustomerPlansUnitOfWork>();
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        services.AddScoped<IBeneficiaryAuditLogRepository, BeneficiaryAuditLogRepository>();

        // Gateways y adaptadores de integración
        services.AddScoped<ICustomerExistenceChecker, CustomerExistenceChecker>();
        services.AddScoped<IFinancialsPlanGateway, FinancialsPlanGatewayStub>();
        services.AddScoped<IAssignedBeneficiariesCounter, AssignedBeneficiariesCounterStub>();

        // Broker de mensajería (listo para Kafka / InMemory)
        services.AddScoped<IEventPublisher, InMemoryEventPublisher>();

        // Almacén Mock en memoria
        services.AddSingleton<InMemoryCustomerStore>();

        // Seeders
        services.AddScoped<IDataSeeder, SubscriptionSeeder>();

        return services;
    }
}
