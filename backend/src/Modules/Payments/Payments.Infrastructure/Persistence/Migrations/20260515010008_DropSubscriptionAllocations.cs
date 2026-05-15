using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropSubscriptionAllocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionAllocationLines",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "SubscriptionAllocationRuns",
                schema: "payments");

            migrationBuilder.DropColumn(
                name: "AllocationLinesCount",
                schema: "payments",
                table: "PayoutRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllocationLinesCount",
                schema: "payments",
                table: "PayoutRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SubscriptionAllocationLines",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AllocationWeight = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    AvailableAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedLessons = table.Column<int>(type: "integer", nullable: false),
                    CompletedSessionsCount = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaidOutAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PayoutRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlatformCommissionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProgressPercent = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    ProviderFeeAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubscriptionAllocationRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TeacherName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TotalLessons = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionAllocationLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionAllocationRuns",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CourseCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlanName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PlatformCommissionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProviderFeeAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Strategy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubscriptionInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherCount = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UserSubscriptionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionAllocationRuns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationLines_PayoutRecordId",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                column: "PayoutRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationLines_SubscriptionAllocationRunId",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                column: "SubscriptionAllocationRunId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationLines_SubscriptionAllocationRunId_Cou~",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                columns: new[] { "SubscriptionAllocationRunId", "CourseId", "TeacherId", "BookingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationLines_TeacherId_CreatedAt",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                columns: new[] { "TeacherId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationRuns_SubscriptionInvoiceId_Strategy",
                schema: "payments",
                table: "SubscriptionAllocationRuns",
                columns: new[] { "SubscriptionInvoiceId", "Strategy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationRuns_UserId_CreatedAt",
                schema: "payments",
                table: "SubscriptionAllocationRuns",
                columns: new[] { "UserId", "CreatedAt" });
        }
    }
}
