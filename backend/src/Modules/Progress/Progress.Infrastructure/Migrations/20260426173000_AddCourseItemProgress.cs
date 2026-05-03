using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Progress.Infrastructure.Persistence;

#nullable disable

namespace Progress.Infrastructure.Migrations
{
    [DbContext(typeof(ProgressDbContext))]
    [Migration("20260426173000_AddCourseItemProgress")]
    public partial class AddCourseItemProgress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseItemProgresses",
                schema: "progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StudentId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseItemProgresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseItemProgresses_CourseId",
                schema: "progress",
                table: "CourseItemProgresses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseItemProgresses_CourseItemId_StudentId",
                schema: "progress",
                table: "CourseItemProgresses",
                columns: new[] { "CourseItemId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseItemProgresses_StudentId",
                schema: "progress",
                table: "CourseItemProgresses",
                column: "StudentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseItemProgresses",
                schema: "progress");
        }
    }
}
