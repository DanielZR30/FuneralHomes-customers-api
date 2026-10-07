using FH.Modules.Customer.Domain.Repositories;
using FH.Shared.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using CustomerEntity = FH.Shared.Domain.Entities.Customer;

namespace FH.Modules.Customer.Infrastructure.Repositories;

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
            c => c.IdentificationType == type && c.IdentificationNumber == number,
            cancellationToken);
    }

    public async Task<CustomerEntity?> GetByIdentificationAsync(
        string identificationType,
        string identificationNumber,
        CancellationToken cancellationToken = default)
    {
        var (type, number) = Normalize(identificationType, identificationNumber);

        return await _dbSet.FirstOrDefaultAsync(
            c => c.IdentificationType == type && c.IdentificationNumber == number,
            cancellationToken);
    }

    // La entidad Customer normaliza tipo y número al construirse, por lo que las
    // consultas deben usar la misma forma para poder comparar contra la columna.
    private static (string Type, string Number) Normalize(string type, string number) =>
        (type.Trim().ToUpperInvariant(), number.Trim().ToUpperInvariant());
}
