using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tests.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialTests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tests");

            migrationBuilder.CreateTable(
                name: "Tests",
                schema: "tests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TimeLimitMinutes = table.Column<int>(type: "integer", nullable: true),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: true),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShuffleQuestions = table.Column<bool>(type: "boolean", nullable: false),
                    ShuffleAnswers = table.Column<bool>(type: "boolean", nullable: false),
                    ShowCorrectAnswers = table.Column<bool>(type: "boolean", nullable: false),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                schema: "tests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Text = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Tests_TestId",
                        column: x => x.TestId,
                        principalSchema: "tests",
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestAttempts",
                schema: "tests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestAttempts_Tests_TestId",
                        column: x => x.TestId,
                        principalSchema: "tests",
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnswerOptions",
                schema: "tests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    MatchingPairValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerOptions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "tests",
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestResponses",
                schema: "tests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedOptionIds = table.Column<string>(type: "text", nullable: true),
                    TextAnswer = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: true),
                    TeacherComment = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestResponses_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "tests",
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestResponses_TestAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalSchema: "tests",
                        principalTable: "TestAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_QuestionId",
                schema: "tests",
                table: "AnswerOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TestId",
                schema: "tests",
                table: "Questions",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_TestAttempts_StudentId",
                schema: "tests",
                table: "TestAttempts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestAttempts_TestId",
                schema: "tests",
                table: "TestAttempts",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_TestAttempts_TestId_StudentId",
                schema: "tests",
                table: "TestAttempts",
                columns: new[] { "TestId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TestResponses_AttemptId",
                schema: "tests",
                table: "TestResponses",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResponses_AttemptId_QuestionId",
                schema: "tests",
                table: "TestResponses",
                columns: new[] { "AttemptId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestResponses_QuestionId",
                schema: "tests",
                table: "TestResponses",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_CreatedById",
                schema: "tests",
                table: "Tests",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerOptions",
                schema: "tests");

            migrationBuilder.DropTable(
                name: "TestResponses",
                schema: "tests");

            migrationBuilder.DropTable(
                name: "Questions",
                schema: "tests");

            migrationBuilder.DropTable(
                name: "TestAttempts",
                schema: "tests");

            migrationBuilder.DropTable(
                name: "Tests",
                schema: "tests");
        }
    }
}
