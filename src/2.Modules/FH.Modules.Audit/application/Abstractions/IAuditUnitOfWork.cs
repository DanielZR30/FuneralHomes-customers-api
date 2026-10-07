namespace FH.Modules.Audit.Application.Abstractions;

public interface IAuditUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
