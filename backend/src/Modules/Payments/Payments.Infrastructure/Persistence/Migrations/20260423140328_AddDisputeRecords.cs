using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDisputeRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DisputedGrossAmount",
                schema: "payments",
                table: "TeacherSettlements",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DisputedNetAmount",
                schema: "payments",
                table: "TeacherSettlements",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "DisputeRecords",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoursePurchaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    TeacherSettlementId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayoutRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TeacherId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CourseTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderDisputeId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProviderPaymentIntentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AppliedGrossAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TeacherNetDisputeAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EvidenceDueBy = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FundsWithdrawnAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FundsReinstatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LedgerAppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LedgerRestoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisputeRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisputeRecords_PaymentAttemptId",
                schema: "payments",
                table: "DisputeRecords",
                column: "PaymentAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_DisputeRecords_ProviderDisputeId",
                schema: "payments",
                table: "DisputeRecords",
                column: "ProviderDisputeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DisputeRecords_StudentId_OpenedAt",
                schema: "payments",
                table: "DisputeRecords",
                columns: new[] { "StudentId", "OpenedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisputeRecords",
                schema: "payments");

            migrationBuilder.DropColumn(
                name: "DisputedGrossAmount",
                schema: "payments",
                table: "TeacherSettlements");

            migrationBuilder.DropColumn(
                name: "DisputedNetAmount",
                schema: "payments",
                table: "TeacherSettlements");
        }
    }
}
