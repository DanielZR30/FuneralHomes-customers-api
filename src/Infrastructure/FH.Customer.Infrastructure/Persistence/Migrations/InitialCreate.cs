using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FH.Shared.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "customers",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                customer_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                identification_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                identification_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE"),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_customers", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "members",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                subject_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                identification_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                identification_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                derived_age = table.Column<int>(type: "integer", nullable: false),
                email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_members", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "customer_subscriptions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                external_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                max_beneficiaries = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                start_date = table.Column<DateOnly>(type: "date", nullable: false),
                end_date = table.Column<DateOnly>(type: "date", nullable: true),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_customer_subscriptions", x => x.id);
                table.ForeignKey(
                    name: "FK_customer_subscriptions_customers_customer_id",
                    column: x => x.customer_id,
                    principalTable: "customers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "beneficiaries",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                beneficiary_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                relationship_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE"),
                joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                removed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_beneficiaries", x => x.id);
                table.ForeignKey(
                    name: "FK_beneficiaries_customer_subscriptions_subscription_id",
                    column: x => x.subscription_id,
                    principalTable: "customer_subscriptions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_beneficiaries_members_member_id",
                    column: x => x.member_id,
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "beneficiary_audit_log",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                member_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                event_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_beneficiary_audit_log", x => x.id);
                table.ForeignKey(
                    name: "FK_beneficiary_audit_log_customer_subscriptions_subscription_id",
                    column: x => x.subscription_id,
                    principalTable: "customer_subscriptions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_beneficiary_audit_log_members_member_id",
                    column: x => x.member_id,
                    principalTable: "members",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_customers_identification_number",
            table: "customers",
            column: "identification_number",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_customer_subscriptions_customer_id",
            table: "customer_subscriptions",
            column: "customer_id");

        migrationBuilder.CreateIndex(
            name: "IX_beneficiaries_member_id",
            table: "beneficiaries",
            column: "member_id");

        migrationBuilder.CreateIndex(
            name: "IX_beneficiaries_subscription_id",
            table: "beneficiaries",
            column: "subscription_id");

        migrationBuilder.CreateIndex(
            name: "IX_beneficiary_audit_log_member_id",
            table: "beneficiary_audit_log",
            column: "member_id");

        migrationBuilder.CreateIndex(
            name: "IX_beneficiary_audit_log_subscription_id",
            table: "beneficiary_audit_log",
            column: "subscription_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "beneficiary_audit_log");
        migrationBuilder.DropTable(name: "beneficiaries");
        migrationBuilder.DropTable(name: "customer_subscriptions");
        migrationBuilder.DropTable(name: "members");
        migrationBuilder.DropTable(name: "customers");
    }
}
