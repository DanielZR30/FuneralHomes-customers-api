using System.Reflection;
using FH.Customer.Api.Infrastructure;
using FH.Customer.Api.Infrastructure.Repositories;
using FH.Modules.Audit.Domain.Repositories;
using FH.Modules.Audit.Infrastructure.Repositories;
using FH.Modules.Beneficiaries.Domain.Repositories;
using FH.Modules.Beneficiaries.Infrastructure.Repositories;
using FH.Modules.Customer.Domain.Repositories;
using FH.Modules.Customer.Infrastructure.Repositories;
using FH.Modules.CustomerPlans.Application.Abstractions;
using FH.Modules.CustomerPlans.Infrastructure;
using FH.Modules.CustomerPlans.Infrastructure.Gateways;
using FH.Shared.Application.Behaviors;
using FH.Shared.Domain.Repositories;
using FH.Shared.Infrastructure.Persistence.Repositories;
using FH.Shared.MockData;
using FH.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FH.Customer.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registro de MediatR para CQRS
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // Repositorios e interfaces de dominio
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

        // Gateways y adaptadores
        services.AddScoped<ICustomerExistenceChecker, CustomerExistenceChecker>();
        services.AddScoped<IFinancialsPlanGateway, FinancialsPlanGatewayStub>();
        services.AddScoped<IAssignedBeneficiariesCounter, AssignedBeneficiariesCounterStub>();

        // Almacén Mock en memoria
        services.AddSingleton<InMemoryCustomerStore>();

        // Seeders
        services.AddScoped<IDataSeeder, SubscriptionSeeder>();

        return services;
    }
}
