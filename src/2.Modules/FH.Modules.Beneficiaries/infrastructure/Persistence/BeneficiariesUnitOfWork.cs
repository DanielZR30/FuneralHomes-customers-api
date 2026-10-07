using FH.Modules.Beneficiaries.Application.Abstractions;
using FH.Modules.Beneficiaries.Infrastructure.Persistence;

namespace FH.Modules.Beneficiaries.Infrastructure.Persistence;

public class BeneficiariesUnitOfWork : IBeneficiariesUnitOfWork
{
    private readonly BeneficiariesDbContext _context;

    public BeneficiariesUnitOfWork(BeneficiariesDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
