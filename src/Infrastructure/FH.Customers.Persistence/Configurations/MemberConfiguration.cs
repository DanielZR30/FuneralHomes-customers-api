using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id");

        builder.Property(m => m.SubjectType)
            .HasColumnName("subject_type")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Enum.Parse<SubjectType>(v, true))
            .IsRequired();

        builder.Property(m => m.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100);

        // Documento opcional (null cuando no se informa).
        builder.OwnsOne(m => m.Document, d =>
        {
            d.Property(x => x.Type)
                .HasColumnName("identification_type")
                .HasMaxLength(20)
                .IsRequired();

            d.Property(x => x.Number)
                .HasColumnName("identification_number")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.Property(m => m.BirthDate)
            .HasColumnName("birth_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(m => m.DerivedAge)
            .HasColumnName("derived_age")
            .IsRequired();

        builder.ComplexProperty(m => m.Contact, contact =>
        {
            contact.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
            contact.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(30);
        });
    }
}
