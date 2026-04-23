using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tests.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTestCourseId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                schema: "tests",
                table: "Tests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tests_CourseId",
                schema: "tests",
                table: "Tests",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tests_CourseId",
                schema: "tests",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "CourseId",
                schema: "tests",
                table: "Tests");
        }
    }
}
