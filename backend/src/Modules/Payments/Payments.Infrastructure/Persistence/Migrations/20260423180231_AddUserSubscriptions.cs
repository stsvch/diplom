using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionPaymentAttempts",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    PlanName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    BillingInterval = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BillingIntervalCount = table.Column<int>(type: "integer", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderCustomerId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderSessionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderSubscriptionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPaymentAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderCustomerId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProviderSubscriptionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PlanName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CurrentPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelAtPeriodEnd = table.Column<bool>(type: "boolean", nullable: false),
                    CanceledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPaymentAttempts_ProviderSessionId",
                schema: "payments",
                table: "SubscriptionPaymentAttempts",
                column: "ProviderSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPaymentAttempts_ProviderSubscriptionId",
                schema: "payments",
                table: "SubscriptionPaymentAttempts",
                column: "ProviderSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPaymentAttempts_UserId_CreatedAt",
                schema: "payments",
                table: "SubscriptionPaymentAttempts",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_ProviderSubscriptionId",
                schema: "payments",
                table: "UserSubscriptions",
                column: "ProviderSubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId_Status",
                schema: "payments",
                table: "UserSubscriptions",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId_SubscriptionPlanId",
                schema: "payments",
                table: "UserSubscriptions",
                columns: new[] { "UserId", "SubscriptionPlanId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionPaymentAttempts",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "UserSubscriptions",
                schema: "payments");
        }
    }
}
