using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentCourseId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                schema: "assignments",
                table: "Assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_CourseId",
                schema: "assignments",
                table: "Assignments",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assignments_CourseId",
                schema: "assignments",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "CourseId",
                schema: "assignments",
                table: "Assignments");
        }
    }
}
