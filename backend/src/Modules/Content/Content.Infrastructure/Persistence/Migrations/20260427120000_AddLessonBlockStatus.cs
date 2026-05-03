using Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ContentDbContext))]
    [Migration("20260427120000_AddLessonBlockStatus")]
    public partial class AddLessonBlockStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "content",
                table: "LessonBlocks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Ready");

            migrationBuilder.AddColumn<string>(
                name: "ValidationErrorsJson",
                schema: "content",
                table: "LessonBlocks",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonBlocks_Status",
                schema: "content",
                table: "LessonBlocks",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LessonBlocks_Status",
                schema: "content",
                table: "LessonBlocks");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "content",
                table: "LessonBlocks");

            migrationBuilder.DropColumn(
                name: "ValidationErrorsJson",
                schema: "content",
                table: "LessonBlocks");
        }
    }
}
