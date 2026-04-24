using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeExerciseRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodeExerciseRuns",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: true),
                    Kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Ok = table.Column<bool>(type: "boolean", nullable: false),
                    GlobalError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Results = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeExerciseRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeExerciseRuns_LessonBlockAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalSchema: "content",
                        principalTable: "LessonBlockAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CodeExerciseRuns_LessonBlocks_BlockId",
                        column: x => x.BlockId,
                        principalSchema: "content",
                        principalTable: "LessonBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeExerciseRuns_AttemptId",
                schema: "content",
                table: "CodeExerciseRuns",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeExerciseRuns_BlockId_UserId_CreatedAt",
                schema: "content",
                table: "CodeExerciseRuns",
                columns: new[] { "BlockId", "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeExerciseRuns",
                schema: "content");
        }
    }
}
