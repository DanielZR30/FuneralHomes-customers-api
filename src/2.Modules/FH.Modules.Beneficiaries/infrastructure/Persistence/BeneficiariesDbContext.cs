using FH.Modules.Beneficiaries.Domain.Entities;
using FH.Modules.Beneficiaries.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FH.Modules.Beneficiaries.Infrastructure.Persistence;

public class BeneficiariesDbContext : DbContext
{
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<Member> Members => Set<Member>();

    public BeneficiariesDbContext(DbContextOptions<BeneficiariesDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new MemberConfiguration());
        modelBuilder.ApplyConfiguration(new BeneficiaryConfiguration());
    }
}
