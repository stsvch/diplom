using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherSettlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeacherSettlements",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StudentId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    StudentName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PaymentAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoursePurchaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProviderFeeAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PlatformCommissionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AvailableAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaidOutAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherSettlements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSettlements_CoursePurchaseId",
                schema: "payments",
                table: "TeacherSettlements",
                column: "CoursePurchaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSettlements_PaymentAttemptId",
                schema: "payments",
                table: "TeacherSettlements",
                column: "PaymentAttemptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSettlements_TeacherId_AvailableAt",
                schema: "payments",
                table: "TeacherSettlements",
                columns: new[] { "TeacherId", "AvailableAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherSettlements",
                schema: "payments");
        }
    }
}
