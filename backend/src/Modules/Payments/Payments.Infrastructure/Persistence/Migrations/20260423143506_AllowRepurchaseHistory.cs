using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowRepurchaseHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CoursePurchases_StudentId_CourseId",
                schema: "payments",
                table: "CoursePurchases");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePurchases_PaymentAttemptId",
                schema: "payments",
                table: "CoursePurchases",
                column: "PaymentAttemptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoursePurchases_StudentId_CourseId",
                schema: "payments",
                table: "CoursePurchases",
                columns: new[] { "StudentId", "CourseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CoursePurchases_PaymentAttemptId",
                schema: "payments",
                table: "CoursePurchases");

            migrationBuilder.DropIndex(
                name: "IX_CoursePurchases_StudentId_CourseId",
                schema: "payments",
                table: "CoursePurchases");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePurchases_StudentId_CourseId",
                schema: "payments",
                table: "CoursePurchases",
                columns: new[] { "StudentId", "CourseId" },
                unique: true);
        }
    }
}
