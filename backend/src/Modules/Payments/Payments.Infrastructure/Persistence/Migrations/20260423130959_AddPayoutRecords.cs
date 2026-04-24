using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayoutRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PayoutRecordId",
                schema: "payments",
                table: "TeacherSettlements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PayoutRecords",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderAccountId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderPayoutId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SettlementsCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSettlements_PayoutRecordId",
                schema: "payments",
                table: "TeacherSettlements",
                column: "PayoutRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRecords_ProviderPayoutId",
                schema: "payments",
                table: "PayoutRecords",
                column: "ProviderPayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRecords_TeacherId_RequestedAt",
                schema: "payments",
                table: "PayoutRecords",
                columns: new[] { "TeacherId", "RequestedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayoutRecords",
                schema: "payments");

            migrationBuilder.DropIndex(
                name: "IX_TeacherSettlements_PayoutRecordId",
                schema: "payments",
                table: "TeacherSettlements");

            migrationBuilder.DropColumn(
                name: "PayoutRecordId",
                schema: "payments",
                table: "TeacherSettlements");
        }
    }
}
