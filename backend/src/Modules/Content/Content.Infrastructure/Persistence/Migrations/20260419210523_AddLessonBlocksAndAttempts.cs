using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonBlocksAndAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LessonBlocks",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false),
                    Settings = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonBlocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LessonBlockAttempts",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Answers = table.Column<string>(type: "jsonb", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    NeedsReview = table.Column<bool>(type: "boolean", nullable: false),
                    AttemptsUsed = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewerComment = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonBlockAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonBlockAttempts_LessonBlocks_BlockId",
                        column: x => x.BlockId,
                        principalSchema: "content",
                        principalTable: "LessonBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonBlockAttempts_BlockId_UserId",
                schema: "content",
                table: "LessonBlockAttempts",
                columns: new[] { "BlockId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonBlockAttempts_UserId",
                schema: "content",
                table: "LessonBlockAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonBlocks_LessonId_OrderIndex",
                schema: "content",
                table: "LessonBlocks",
                columns: new[] { "LessonId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonBlocks_Type",
                schema: "content",
                table: "LessonBlocks",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonBlockAttempts",
                schema: "content");

            migrationBuilder.DropTable(
                name: "LessonBlocks",
                schema: "content");
        }
    }
}
