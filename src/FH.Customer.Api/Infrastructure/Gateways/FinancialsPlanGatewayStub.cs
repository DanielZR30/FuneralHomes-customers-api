using FH.Modules.CustomerPlans.Application.Abstractions;

namespace FH.Modules.CustomerPlans.Infrastructure.Gateways;

/// <summary>
/// Implementación Stub del gateway de planes financieros (RN-08).
/// TODO: Pendiente de integración con el microservicio Financials (fh.api.financial).
/// Acepta cualquier UUID de plan para propósitos de desarrollo y desacoplamiento.
/// </summary>
public class FinancialsPlanGatewayStub : IFinancialsPlanGateway
{
    public Task<bool> PlanExistsAsync(Guid externalPlanId, CancellationToken cancellationToken = default)
    {
        // Acepta cualquier GUID no vacío como plan válido del microservicio Financials
        var isValid = externalPlanId != Guid.Empty;
        return Task.FromResult(isValid);
    }
}
