namespace FH.Modules.CustomerPlans.Application.Abstractions;

public interface IFinancialsPlanGateway
{
    /// <summary>
    /// Verifica si el identificador del plan comercial existe en el microservicio Financials (RN-08).
    /// </summary>
    Task<bool> PlanExistsAsync(Guid externalPlanId, CancellationToken cancellationToken = default);
}
