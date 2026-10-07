using FH.Modules.Beneficiaries.Domain.Entities;
using FH.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FH.Modules.Beneficiaries.Infrastructure.Configurations;

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

        builder.Property(m => m.IdentificationType)
            .HasColumnName("identification_type")
            .HasMaxLength(20);

        builder.Property(m => m.IdentificationNumber)
            .HasColumnName("identification_number")
            .HasMaxLength(50);

        builder.Property(m => m.BirthDate)
            .HasColumnName("birth_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(m => m.DerivedAge)
            .HasColumnName("derived_age")
            .IsRequired();

        builder.Property(m => m.Email)
            .HasColumnName("email")
            .HasMaxLength(100);

        builder.Property(m => m.Phone)
            .HasColumnName("phone")
            .HasMaxLength(30);
    }
}
