using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;

namespace FH.Customers.Persistence.Configurations;

public class BeneficiaryAuditLogConfiguration : IEntityTypeConfiguration<BeneficiaryAuditLog>
{
    public void Configure(EntityTypeBuilder<BeneficiaryAuditLog> builder)
    {
        builder.ToTable("beneficiary_audit_log");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.SubscriptionId)
            .HasColumnName("subscription_id")
            .IsRequired();

        builder.Property(a => a.MemberId)
            .HasColumnName("member_id")
            .IsRequired();

        var actionConverter = new ValueConverter<AuditAction, string>(
            v => v == AuditAction.BeneficiaryAdded ? "BENEFICIARY_ADDED" :
                 v == AuditAction.BeneficiaryRemoved ? "BENEFICIARY_REMOVED" :
                 v == AuditAction.BeneficiaryUpdated ? "BENEFICIARY_UPDATED" : v.ToString().ToUpperInvariant(),
            v => v == "BENEFICIARY_ADDED" ? AuditAction.BeneficiaryAdded :
                 v == "BENEFICIARY_REMOVED" ? AuditAction.BeneficiaryRemoved :
                 v == "BENEFICIARY_UPDATED" ? AuditAction.BeneficiaryUpdated :
                 Enum.Parse<AuditAction>(v, true));

        builder.Property(a => a.Action)
            .HasColumnName("action")
            .HasMaxLength(30)
            .HasConversion(actionConverter)
            .IsRequired();

        builder.Property(a => a.EventPublished)
            .HasColumnName("event_published")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasOne(a => a.Subscription)
            .WithMany()
            .HasForeignKey(a => a.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Member)
            .WithMany()
            .HasForeignKey(a => a.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
