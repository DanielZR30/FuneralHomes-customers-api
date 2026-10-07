using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Persistence.Configurations;

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

        // Value objects: se guardan en la misma tabla `customers`.
        builder.OwnsOne(c => c.Document, d =>
        {
            d.Property(x => x.Type)
                .HasColumnName("identification_type")
                .HasMaxLength(20)
                .IsRequired();

            d.Property(x => x.Number)
                .HasColumnName("identification_number")
                .HasMaxLength(50)
                .IsRequired();

            // La clave de negocio es la pareja tipo + número.
            d.HasIndex(x => new { x.Type, x.Number }).IsUnique();
        });
        builder.Navigation(c => c.Document).IsRequired();

        builder.ComplexProperty(c => c.Contact, contact =>
        {
            contact.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            contact.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(30);
        });

        builder.OwnsOne(c => c.Address, a =>
        {
            a.Property(x => x.Value)
                .HasColumnName("address")
                .HasMaxLength(255)
                .IsRequired();
        });

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
