using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.Abstractions;
using FH.Customers.Infrastructure.CustomerPlans.Gateways;
using FH.Customers.Infrastructure.MockData;

namespace FH.Customers.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra la capa Infrastructure: adaptadores a sistemas externos (gateways), almacén mock
    /// en memoria y, más adelante, el publicador de eventos (Kafka).
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // Almacén Mock en memoria (permite probar la API sin PostgreSQL levantado)
        services.AddSingleton<InMemoryCustomerStore>();

        // Gateways y adaptadores de puertos
        services.AddScoped<ICustomerExistenceChecker, CustomerExistenceChecker>();
        services.AddScoped<IFinancialsPlanGateway, FinancialsPlanGatewayStub>();
        services.AddScoped<IAssignedBeneficiariesCounter, AssignedBeneficiariesCounterStub>();

        // Mensajería (IEventPublisher + Kafka): pendiente de implementar

        return services;
    }
}
