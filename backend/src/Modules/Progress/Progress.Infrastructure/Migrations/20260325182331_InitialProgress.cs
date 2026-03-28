using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Progress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "progress");

            migrationBuilder.CreateTable(
                name: "LessonProgresses",
                schema: "progress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonProgresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonProgresses_LessonId_StudentId",
                schema: "progress",
                table: "LessonProgresses",
                columns: new[] { "LessonId", "StudentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonProgresses",
                schema: "progress");
        }
    }
}
