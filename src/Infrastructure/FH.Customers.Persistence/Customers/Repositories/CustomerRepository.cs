using Microsoft.EntityFrameworkCore;
using CustomerEntity = FH.Customers.Domain.Entities.Customer;
using FH.Customers.Domain.Customers.Repositories;
using FH.Customers.Persistence.Repositories;

namespace FH.Customers.Persistence.Customers.Repositories;

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
        var (type, number) = Normalize(identificationType, identificationNumber);

        return await _dbSet.AnyAsync(
            c => c.Document.Type == type && c.Document.Number == number,
            cancellationToken);
    }

    public async Task<CustomerEntity?> GetByIdentificationAsync(
        string identificationType,
        string identificationNumber,
        CancellationToken cancellationToken = default)
    {
        var (type, number) = Normalize(identificationType, identificationNumber);

        return await _dbSet.FirstOrDefaultAsync(
            c => c.Document.Type == type && c.Document.Number == number,
            cancellationToken);
    }

    // La entidad Customer normaliza tipo y número al construirse, por lo que las
    // consultas deben usar la misma forma para poder comparar contra la columna.
    private static (string Type, string Number) Normalize(string type, string number) =>
        (type.Trim().ToUpperInvariant(), number.Trim().ToUpperInvariant());
}
