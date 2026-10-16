using Microsoft.EntityFrameworkCore;
using CustomerEntity = FH.Customers.Domain.Entities.Customer;
using FH.Customers.Application.Contracts.Repositories;
using FH.Customers.Domain.Common.ValueObjects;

namespace FH.Customers.Persistence.Repositories;

public class CustomerRepository : EfRepository<CustomerEntity, Guid>, ICustomerRepository
{
    public CustomerRepository(DbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByIdentificationAsync(
        string identificationType,
        string identificationNumber,
        CancellationToken cancellationToken = default)
    {
        if (!DocumentId.TryParseType(identificationType, out var type))
            return false;

        var number = Normalize(identificationNumber);

        return await _dbSet.AnyAsync(
            c => c.Document.Type == type && c.Document.Number == number,
            cancellationToken);
    }

    public async Task<CustomerEntity?> GetByIdentificationAsync(
        string identificationType,
        string identificationNumber,
        CancellationToken cancellationToken = default)
    {
        if (!DocumentId.TryParseType(identificationType, out var type))
            return null;

        var number = Normalize(identificationNumber);

        return await _dbSet.FirstOrDefaultAsync(
            c => c.Document.Type == type && c.Document.Number == number,
            cancellationToken);
    }

    // DocumentId normaliza el número al construirse (y el tipo es un enum), por lo que las
    // consultas deben usar la misma forma para poder comparar contra la columna.
    private static string Normalize(string number) => number.Trim().ToUpperInvariant();
}
