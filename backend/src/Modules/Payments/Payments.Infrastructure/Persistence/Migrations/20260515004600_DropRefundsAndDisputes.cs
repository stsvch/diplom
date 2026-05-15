using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropRefundsAndDisputes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisputeRecords",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "RefundRecords",
                schema: "payments");

            migrationBuilder.DropColumn(
                name: "DisputedGrossAmount",
                schema: "payments",
                table: "TeacherSettlements");

            migrationBuilder.DropColumn(
                name: "DisputedNetAmount",
                schema: "payments",
                table: "TeacherSettlements");

            migrationBuilder.DropColumn(
                name: "RefundedGrossAmount",
                schema: "payments",
                table: "TeacherSettlements");

            migrationBuilder.DropColumn(
                name: "RefundedNetAmount",
                schema: "payments",
                table: "TeacherSettlements");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<decimal>(
                name: "RefundedGrossAmount",
                schema: "payments",
                table: "TeacherSettlements",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundedNetAmount",
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
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AppliedGrossAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CoursePurchaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    CourseTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    EvidenceDueBy = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FundsReinstatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FundsWithdrawnAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LedgerAppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LedgerRestoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayoutRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderDisputeId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProviderPaymentIntentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StudentId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TeacherId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TeacherNetDisputeAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TeacherSettlementId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisputeRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefundRecords",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CoursePurchaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    CourseTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FailureMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LedgerAppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayoutRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderPaymentIntentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderRefundId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestedByAdminId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StudentId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TeacherId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TeacherNetRefundAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TeacherSettlementId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRecords", x => x.Id);
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

            migrationBuilder.CreateIndex(
                name: "IX_RefundRecords_PaymentAttemptId",
                schema: "payments",
                table: "RefundRecords",
                column: "PaymentAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRecords_ProviderRefundId",
                schema: "payments",
                table: "RefundRecords",
                column: "ProviderRefundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefundRecords_StudentId_RequestedAt",
                schema: "payments",
                table: "RefundRecords",
                columns: new[] { "StudentId", "RequestedAt" });
        }
    }
}
