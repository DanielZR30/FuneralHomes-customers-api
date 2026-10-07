using CustomerEntity = FH.Customers.Domain.Entities.Customer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.CustomerPlans.ValueObjects;
using FH.Customers.Domain.Enums;
using FH.Customers.Persistence.CustomerPlans;

namespace FH.Customers.Persistence.CustomerPlans.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("customer_subscriptions", t =>
        {
            t.HasCheckConstraint("CK_customer_subscriptions_max_beneficiaries", "max_beneficiaries >= 1");
            t.HasCheckConstraint("CK_customer_subscriptions_dates", "end_date IS NULL OR end_date >= start_date");
            t.HasCheckConstraint("CK_customer_subscriptions_status", "status IN ('ACTIVE', 'SUSPENDED', 'CANCELLED', 'EXPIRED')");
        });

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
            .HasDefaultValue(new MaxBeneficiaries(1))
            .HasConversion(
                v => v.Value,
                v => new MaxBeneficiaries(v))
            .IsRequired();

        builder.ComplexProperty(s => s.Period, periodBuilder =>
        {
            periodBuilder.Property(p => p.StartDate)
                .HasColumnName("start_date")
                .HasColumnType("date")
                .IsRequired();

            periodBuilder.Property(p => p.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("date");
        });

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Enum.Parse<SubscriptionStatus>(v, true))
            .HasDefaultValue(SubscriptionStatus.Active)
            .IsRequired();

        builder.HasIndex(s => s.CustomerId)
            .HasDatabaseName("IX_customer_subscriptions_customer_id");

        builder.HasOne<CustomerEntity>()
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(s => s.DomainEvents);
    }
}
