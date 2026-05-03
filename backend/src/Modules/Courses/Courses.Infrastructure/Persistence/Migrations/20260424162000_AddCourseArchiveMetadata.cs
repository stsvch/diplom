using Courses.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Courses.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(CoursesDbContext))]
    [Migration("20260424162000_AddCourseArchiveMetadata")]
    public partial class AddCourseArchiveMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArchivedBy",
                schema: "courses",
                table: "Courses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchiveReason",
                schema: "courses",
                table: "Courses",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedBy",
                schema: "courses",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ArchiveReason",
                schema: "courses",
                table: "Courses");
        }
    }
}
