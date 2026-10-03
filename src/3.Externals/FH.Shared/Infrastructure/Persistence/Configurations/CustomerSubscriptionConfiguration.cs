using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FH.Shared.Infrastructure.Persistence.Configurations;

public class CustomerSubscriptionConfiguration : IEntityTypeConfiguration<CustomerSubscription>
{
    public void Configure(EntityTypeBuilder<CustomerSubscription> builder)
    {
        builder.ToTable("customer_subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(s => s.ExternalPlanId)
            .HasColumnName("external_plan_id")
            .IsRequired();

        builder.Property(s => s.MaxBeneficiaries)
            .HasColumnName("max_beneficiaries")
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(s => s.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(s => s.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("date");

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Enum.Parse<SubscriptionStatus>(v, true))
            .HasDefaultValue(SubscriptionStatus.Active)
            .IsRequired();

        builder.HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(s => s.DomainEvents);
    }
}
