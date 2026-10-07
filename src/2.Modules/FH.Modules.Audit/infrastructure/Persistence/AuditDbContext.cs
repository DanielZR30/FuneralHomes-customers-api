using FH.Modules.Audit.Domain.Entities;
using FH.Modules.Audit.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FH.Modules.Audit.Infrastructure.Persistence;

public class AuditDbContext : DbContext
{
    public DbSet<BeneficiaryAuditLog> BeneficiaryAuditLogs => Set<BeneficiaryAuditLog>();

    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new BeneficiaryAuditLogConfiguration());
    }
}
