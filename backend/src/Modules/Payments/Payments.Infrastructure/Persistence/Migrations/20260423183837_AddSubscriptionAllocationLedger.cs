using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionAllocationLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionAllocationLines",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionAllocationRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TeacherId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TeacherName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AllocationWeight = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    ProgressPercent = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    TotalLessons = table.Column<int>(type: "integer", nullable: false),
                    CompletedLessons = table.Column<int>(type: "integer", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PlatformCommissionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProviderFeeAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AllocatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    SubscriptionInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserSubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    PlanName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PlatformCommissionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProviderFeeAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Strategy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TeacherCount = table.Column<int>(type: "integer", nullable: false),
                    CourseCount = table.Column<int>(type: "integer", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AllocatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionAllocationRuns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationLines_SubscriptionAllocationRunId",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                column: "SubscriptionAllocationRunId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationLines_SubscriptionAllocationRunId_Cou~",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                columns: new[] { "SubscriptionAllocationRunId", "CourseId", "TeacherId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationLines_TeacherId_CreatedAt",
                schema: "payments",
                table: "SubscriptionAllocationLines",
                columns: new[] { "TeacherId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationRuns_SubscriptionInvoiceId",
                schema: "payments",
                table: "SubscriptionAllocationRuns",
                column: "SubscriptionInvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocationRuns_UserId_CreatedAt",
                schema: "payments",
                table: "SubscriptionAllocationRuns",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionAllocationLines",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "SubscriptionAllocationRuns",
                schema: "payments");
        }
    }
}
