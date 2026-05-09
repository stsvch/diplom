using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionUsageAndSlotQuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupSlotsPerMonth",
                schema: "payments",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IndividualSlotsPerMonth",
                schema: "payments",
                table: "SubscriptionPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SubscriptionUsages",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceBookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRefunded = table.Column<bool>(type: "boolean", nullable: false),
                    RefundedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionUsages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionUsages_SourceBookingId",
                schema: "payments",
                table: "SubscriptionUsages",
                column: "SourceBookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionUsages_UserId_PeriodStart",
                schema: "payments",
                table: "SubscriptionUsages",
                columns: new[] { "UserId", "PeriodStart" });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionUsages_UserSubscriptionId_PeriodStart_Type",
                schema: "payments",
                table: "SubscriptionUsages",
                columns: new[] { "UserSubscriptionId", "PeriodStart", "Type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionUsages",
                schema: "payments");

            migrationBuilder.DropColumn(
                name: "GroupSlotsPerMonth",
                schema: "payments",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "IndividualSlotsPerMonth",
                schema: "payments",
                table: "SubscriptionPlans");
        }
    }
}
