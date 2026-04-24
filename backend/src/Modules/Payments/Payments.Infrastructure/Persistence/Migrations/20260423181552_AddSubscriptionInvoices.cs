using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionInvoices",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserSubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderInvoiceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProviderSubscriptionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PlanName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AmountDue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BillingReason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionInvoices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionInvoices_ProviderInvoiceId",
                schema: "payments",
                table: "SubscriptionInvoices",
                column: "ProviderInvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionInvoices_ProviderSubscriptionId",
                schema: "payments",
                table: "SubscriptionInvoices",
                column: "ProviderSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionInvoices_UserId_CreatedAt",
                schema: "payments",
                table: "SubscriptionInvoices",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionInvoices",
                schema: "payments");
        }
    }
}
