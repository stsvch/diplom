using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Courses.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentIdToLessonBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignmentId",
                schema: "courses",
                table: "LessonBlocks",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignmentId",
                schema: "courses",
                table: "LessonBlocks");
        }
    }
}
