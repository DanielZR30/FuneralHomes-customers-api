using System;
using FH.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace FH.Shared.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CustomerDbContext))]
partial class CustomerDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.12")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("FH.Shared.Domain.Entities.Customer", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid")
                .HasColumnName("id");

            b.Property<string>("Address")
                .HasMaxLength(255)
                .HasColumnType("character varying(255)")
                .HasColumnName("address");

            b.Property<DateTime>("CreatedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            b.Property<string>("CustomerType")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasColumnName("customer_type");

            b.Property<string>("Email")
                .HasMaxLength(100)
                .HasColumnType("character varying(100)")
                .HasColumnName("email");

            b.Property<string>("IdentificationNumber")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("character varying(50)")
                .HasColumnName("identification_number");

            b.Property<string>("IdentificationType")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasColumnName("identification_type");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("character varying(255)")
                .HasColumnName("name");

            b.Property<string>("Phone")
                .HasMaxLength(30)
                .HasColumnType("character varying(30)")
                .HasColumnName("phone");

            b.Property<string>("Status")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");

            b.HasKey("Id");

            b.HasIndex("IdentificationNumber")
                .IsUnique();

            b.ToTable("customers", (string)null);
        });

        modelBuilder.Entity("FH.Shared.Domain.Entities.CustomerSubscription", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid")
                .HasColumnName("id");

            b.Property<Guid>("CustomerId")
                .HasColumnType("uuid")
                .HasColumnName("customer_id");

            b.Property<DateOnly?>("EndDate")
                .HasColumnType("date")
                .HasColumnName("end_date");

            b.Property<Guid>("ExternalPlanId")
                .HasColumnType("uuid")
                .HasColumnName("external_plan_id");

            b.Property<int>("MaxBeneficiaries")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasDefaultValue(1)
                .HasColumnName("max_beneficiaries");

            b.Property<DateOnly>("StartDate")
                .HasColumnType("date")
                .HasColumnName("start_date");

            b.Property<string>("Status")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");

            b.HasKey("Id");

            b.HasIndex("CustomerId");

            b.ToTable("customer_subscriptions", (string)null);
        });

        modelBuilder.Entity("FH.Shared.Domain.Entities.Member", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid")
                .HasColumnName("id");

            b.Property<DateOnly>("BirthDate")
                .HasColumnType("date")
                .HasColumnName("birth_date");

            b.Property<int>("DerivedAge")
                .HasColumnType("integer")
                .HasColumnName("derived_age");

            b.Property<string>("Email")
                .HasMaxLength(100)
                .HasColumnType("character varying(100)")
                .HasColumnName("email");

            b.Property<string>("FirstName")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)")
                .HasColumnName("first_name");

            b.Property<string>("IdentificationNumber")
                .HasMaxLength(50)
                .HasColumnType("character varying(50)")
                .HasColumnName("identification_number");

            b.Property<string>("IdentificationType")
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasColumnName("identification_type");

            b.Property<string>("LastName")
                .HasMaxLength(100)
                .HasColumnType("character varying(100)")
                .HasColumnName("last_name");

            b.Property<string>("Phone")
                .HasMaxLength(30)
                .HasColumnType("character varying(30)")
                .HasColumnName("phone");

            b.Property<string>("SubjectType")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasColumnName("subject_type");

            b.HasKey("Id");

            b.ToTable("members", (string)null);
        });

        modelBuilder.Entity("FH.Shared.Domain.Entities.Beneficiary", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid")
                .HasColumnName("id");

            b.Property<string>("BeneficiaryType")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasColumnName("beneficiary_type");

            b.Property<DateTime>("JoinedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("joined_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            b.Property<Guid>("MemberId")
                .HasColumnType("uuid")
                .HasColumnName("member_id");

            b.Property<string>("RelationshipType")
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnType("character varying(30)")
                .HasColumnName("relationship_type");

            b.Property<DateTime?>("RemovedAt")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("removed_at");

            b.Property<string>("Status")
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)")
                .HasDefaultValue("ACTIVE")
                .HasColumnName("status");

            b.Property<Guid>("SubscriptionId")
                .HasColumnType("uuid")
                .HasColumnName("subscription_id");

            b.HasKey("Id");

            b.HasIndex("MemberId");

            b.HasIndex("SubscriptionId");

            b.ToTable("beneficiaries", (string)null);
        });

        modelBuilder.Entity("FH.Shared.Domain.Entities.BeneficiaryAuditLog", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid")
                .HasColumnName("id");

            b.Property<string>("Action")
                .IsRequired()
                .HasMaxLength(30)
                .HasColumnType("character varying(30)")
                .HasColumnName("action");

            b.Property<DateTime>("CreatedAt")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            b.Property<bool>("EventPublished")
                .ValueGeneratedOnAdd()
                .HasColumnType("boolean")
                .HasDefaultValue(false)
                .HasColumnName("event_published");

            b.Property<Guid>("MemberId")
                .HasColumnType("uuid")
                .HasColumnName("member_id");

            b.Property<Guid>("SubscriptionId")
                .HasColumnType("uuid")
                .HasColumnName("subscription_id");

            b.HasKey("Id");

            b.HasIndex("MemberId");

            b.HasIndex("SubscriptionId");

            b.ToTable("beneficiary_audit_log", (string)null);
        });

        modelBuilder.Entity("FH.Shared.Domain.Entities.CustomerSubscription", b =>
        {
            b.HasOne("FH.Shared.Domain.Entities.Customer", "Customer")
                .WithMany()
                .HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("Customer");
        });

        modelBuilder.Entity("FH.Shared.Domain.Entities.Beneficiary", b =>
        {
            b.HasOne("FH.Shared.Domain.Entities.Member", "Member")
                .WithMany()
                .HasForeignKey("MemberId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasOne("FH.Shared.Domain.Entities.CustomerSubscription", "Subscription")
                .WithMany()
                .HasForeignKey("SubscriptionId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("Member");
            b.Navigation("Subscription");
        });

        modelBuilder.Entity("FH.Shared.Domain.Entities.BeneficiaryAuditLog", b =>
        {
            b.HasOne("FH.Shared.Domain.Entities.Member", "Member")
                .WithMany()
                .HasForeignKey("MemberId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasOne("FH.Shared.Domain.Entities.CustomerSubscription", "Subscription")
                .WithMany()
                .HasForeignKey("SubscriptionId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("Member");
            b.Navigation("Subscription");
        });
#pragma warning restore 612, 618
    }
}
