using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Courses.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorLessonBlocksAndLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonBlocks",
                schema: "courses");

            migrationBuilder.AddColumn<string>(
                name: "Layout",
                schema: "courses",
                table: "Lessons",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Scroll");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Layout",
                schema: "courses",
                table: "Lessons");

            migrationBuilder.CreateTable(
                name: "LessonBlocks",
                schema: "courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    TestId = table.Column<Guid>(type: "uuid", nullable: true),
                    TextContent = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonBlocks_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalSchema: "courses",
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonBlocks_LessonId",
                schema: "courses",
                table: "LessonBlocks",
                column: "LessonId");
        }
    }
}
