using FH.Modules.Audit.Application.Abstractions;
using FH.Modules.Audit.Infrastructure.Persistence;

namespace FH.Modules.Audit.Infrastructure.Persistence;

public class AuditUnitOfWork : IAuditUnitOfWork
{
    private readonly AuditDbContext _context;

    public AuditUnitOfWork(AuditDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
