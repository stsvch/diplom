using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grading.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialGrading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "grading");

            migrationBuilder.CreateTable(
                name: "Grades",
                schema: "grading",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TestAttemptId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignmentSubmissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Score = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GradedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Grades_CourseId_StudentId",
                schema: "grading",
                table: "Grades",
                columns: new[] { "CourseId", "StudentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Grades",
                schema: "grading");
        }
    }
}
