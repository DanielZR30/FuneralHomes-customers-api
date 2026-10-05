using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FH.Modules.CustomerPlans.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCustomerPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer_subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    max_beneficiaries = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE"),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_subscriptions", x => x.id);
                    table.CheckConstraint("CK_customer_subscriptions_dates", "end_date IS NULL OR end_date >= start_date");
                    table.CheckConstraint("CK_customer_subscriptions_max_beneficiaries", "max_beneficiaries >= 1");
                    table.CheckConstraint("CK_customer_subscriptions_status", "status IN ('ACTIVE', 'SUSPENDED', 'CANCELLED', 'EXPIRED')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_customer_subscriptions_customer_id",
                table: "customer_subscriptions",
                column: "customer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_subscriptions");
        }
    }
}
