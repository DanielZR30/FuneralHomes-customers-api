using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Persistence.Configurations;

public class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
{
    public void Configure(EntityTypeBuilder<Beneficiary> builder)
    {
        builder.ToTable("beneficiaries");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("id");

        builder.Property(b => b.SubscriptionId)
            .HasColumnName("subscription_id")
            .IsRequired();

        builder.Property(b => b.MemberId)
            .HasColumnName("member_id")
            .IsRequired();

        builder.Property(b => b.BeneficiaryType)
            .HasColumnName("beneficiary_type")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Enum.Parse<BeneficiaryType>(v, true))
            .IsRequired();

        builder.Property(b => b.RelationshipType)
            .HasColumnName("relationship_type")
            .HasMaxLength(30)
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Enum.Parse<RelationshipType>(v, true))
            .IsRequired();

        builder.Property(b => b.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Enum.Parse<BeneficiaryStatus>(v, true))
            .HasDefaultValue(BeneficiaryStatus.Active)
            .IsRequired();

        builder.Property(b => b.JoinedAt)
            .HasColumnName("joined_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(b => b.RemovedAt)
            .HasColumnName("removed_at");

        builder.HasOne(b => b.Subscription)
            .WithMany()
            .HasForeignKey(b => b.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Member)
            .WithMany()
            .HasForeignKey(b => b.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(b => b.DomainEvents);
    }
}
