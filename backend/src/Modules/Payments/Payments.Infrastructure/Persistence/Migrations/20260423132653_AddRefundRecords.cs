using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "RefundRecords",
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
                    ProviderRefundId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProviderPaymentIntentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TeacherNetRefundAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LedgerAppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRecords", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefundRecords",
                schema: "payments");

            migrationBuilder.DropColumn(
                name: "RefundedGrossAmount",
                schema: "payments",
                table: "TeacherSettlements");

            migrationBuilder.DropColumn(
                name: "RefundedNetAmount",
                schema: "payments",
                table: "TeacherSettlements");
        }
    }
}
