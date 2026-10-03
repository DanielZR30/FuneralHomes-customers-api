using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FH.Shared.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.CustomerType)
            .HasColumnName("customer_type")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Enum.Parse<CustomerType>(v, true))
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.IdentificationType)
            .HasColumnName("identification_type")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.IdentificationNumber)
            .HasColumnName("identification_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(c => c.IdentificationNumber)
            .IsUnique();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(100);

        builder.Property(c => c.Phone)
            .HasColumnName("phone")
            .HasMaxLength(30);

        builder.Property(c => c.Address)
            .HasColumnName("address")
            .HasMaxLength(255);

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Enum.Parse<CustomerStatus>(v, true))
            .HasDefaultValue(CustomerStatus.Active)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Ignore(c => c.DomainEvents);
    }
}
