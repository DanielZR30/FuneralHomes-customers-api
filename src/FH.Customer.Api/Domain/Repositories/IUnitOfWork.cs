using FH.Shared.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace FH.Shared.Domain.Repositories;

/// <summary>
/// Contrato del patrón Unit of Work para coordinar el trabajo de múltiples repositorios y transacciones.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    IRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : BaseEntity<TId>;

    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
}
