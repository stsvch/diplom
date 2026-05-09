using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveSessionAllocationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubscriptionAllocationRuns_SubscriptionInvoiceId",
                schema: "payments",
                table: "SubscriptionAllocationRuns");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionAllocationLines_SubscriptionAllocationRunId_Cou~",
                schema: "payments",
                table: "SubscriptionAllocationLines");

            migrationBuilder.AddColumn<Guid>(
                name: "BookingId",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletedSessionsCount",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "CourseProgress");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationRuns_SubscriptionInvoiceId_Strategy",
                schema: "payments",
                table: "SubscriptionAllocationRuns",
                columns: new[] { "SubscriptionInvoiceId", "Strategy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationLines_SubscriptionAllocationRunId_Cou~",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                columns: new[] { "SubscriptionAllocationRunId", "CourseId", "TeacherId", "BookingId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubscriptionAllocationRuns_SubscriptionInvoiceId_Strategy",
                schema: "payments",
                table: "SubscriptionAllocationRuns");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionAllocationLines_SubscriptionAllocationRunId_Cou~",
                schema: "payments",
                table: "SubscriptionAllocationLines");

            migrationBuilder.DropColumn(
                name: "BookingId",
                schema: "payments",
                table: "SubscriptionAllocationLines");

            migrationBuilder.DropColumn(
                name: "CompletedSessionsCount",
                schema: "payments",
                table: "SubscriptionAllocationLines");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "payments",
                table: "SubscriptionAllocationLines");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationRuns_SubscriptionInvoiceId",
                schema: "payments",
                table: "SubscriptionAllocationRuns",
                column: "SubscriptionInvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationLines_SubscriptionAllocationRunId_Cou~",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                columns: new[] { "SubscriptionAllocationRunId", "CourseId", "TeacherId" },
                unique: true);
        }
    }
}
