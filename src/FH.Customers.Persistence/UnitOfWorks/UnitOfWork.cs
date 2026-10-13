using Microsoft.EntityFrameworkCore;
using FH.Customers.Application.Contracts.Persistence;
using FH.Customers.Application.Exceptions;

namespace FH.Customers.Persistence.UnitOfWorks;

/// <summary>
/// Unit of Work con EF Core. Los eventos de dominio los despacha CustomerDbContext al guardar.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly CustomerDbContext _context;

    public UnitOfWork(CustomerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new DataConflictException("La base de datos rechazó el guardado (conflicto de datos).", ex);
        }
    }

    // EF Core ya guarda todo o nada en SaveChanges; aquí solo se descartan los cambios pendientes en memoria.
    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _context.ChangeTracker.Clear();
        return Task.CompletedTask;
    }
}
