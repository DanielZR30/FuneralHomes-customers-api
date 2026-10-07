namespace FH.Modules.Beneficiaries.Application.Abstractions;

public interface IBeneficiariesUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
