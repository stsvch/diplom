using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tools.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialToolsGlossary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tools");

            migrationBuilder.CreateTable(
                name: "DictionaryWords",
                schema: "tools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Term = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Translation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Definition = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Example = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Tags = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryWords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDictionaryProgress",
                schema: "tools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WordId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IsKnown = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    LastReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDictionaryProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDictionaryProgress_DictionaryWords_WordId",
                        column: x => x.WordId,
                        principalSchema: "tools",
                        principalTable: "DictionaryWords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryWords_CourseId_Term",
                schema: "tools",
                table: "DictionaryWords",
                columns: new[] { "CourseId", "Term" });

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryWords_CreatedById",
                schema: "tools",
                table: "DictionaryWords",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserDictionaryProgress_UserId",
                schema: "tools",
                table: "UserDictionaryProgress",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDictionaryProgress_WordId_UserId",
                schema: "tools",
                table: "UserDictionaryProgress",
                columns: new[] { "WordId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDictionaryProgress",
                schema: "tools");

            migrationBuilder.DropTable(
                name: "DictionaryWords",
                schema: "tools");
        }
    }
}
