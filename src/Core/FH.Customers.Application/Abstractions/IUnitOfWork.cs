namespace FH.Customers.Application.Abstractions;

/// <summary>
/// Unit of Work: confirma (o descarta) los cambios hechos por los repositorios en una sola operación.
/// No expone nada de Entity Framework; esa parte vive en Persistence.
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}
