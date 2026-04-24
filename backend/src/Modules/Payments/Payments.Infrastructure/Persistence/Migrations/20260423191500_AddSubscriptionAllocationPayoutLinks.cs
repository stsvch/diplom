using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Payments.Infrastructure.Persistence;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(PaymentsDbContext))]
    [Migration("20260423191500_AddSubscriptionAllocationPayoutLinks")]
    public partial class AddSubscriptionAllocationPayoutLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllocationLinesCount",
                schema: "payments",
                table: "PayoutRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableAt",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidOutAt",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PayoutRecordId",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE payments."SubscriptionAllocationLines"
                SET "AvailableAt" = "AllocatedAt"
                WHERE "AvailableAt" IS NULL;
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AvailableAt",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationLines_PayoutRecordId",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                column: "PayoutRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubscriptionAllocationLines_PayoutRecordId",
                schema: "payments",
                table: "SubscriptionAllocationLines");

            migrationBuilder.DropColumn(
                name: "AllocationLinesCount",
                schema: "payments",
                table: "PayoutRecords");

            migrationBuilder.DropColumn(
                name: "AvailableAt",
                schema: "payments",
                table: "SubscriptionAllocationLines");

            migrationBuilder.DropColumn(
                name: "PaidOutAt",
                schema: "payments",
                table: "SubscriptionAllocationLines");

            migrationBuilder.DropColumn(
                name: "PayoutRecordId",
                schema: "payments",
                table: "SubscriptionAllocationLines");
        }
    }
}
