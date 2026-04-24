using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tools.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDictionaryReviewScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HardCount",
                schema: "tools",
                table: "UserDictionaryProgress",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LastOutcome",
                schema: "tools",
                table: "UserDictionaryProgress",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextReviewAt",
                schema: "tools",
                table: "UserDictionaryProgress",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatLaterCount",
                schema: "tools",
                table: "UserDictionaryProgress",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserDictionaryProgress_NextReviewAt",
                schema: "tools",
                table: "UserDictionaryProgress",
                column: "NextReviewAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDictionaryProgress_NextReviewAt",
                schema: "tools",
                table: "UserDictionaryProgress");

            migrationBuilder.DropColumn(
                name: "HardCount",
                schema: "tools",
                table: "UserDictionaryProgress");

            migrationBuilder.DropColumn(
                name: "LastOutcome",
                schema: "tools",
                table: "UserDictionaryProgress");

            migrationBuilder.DropColumn(
                name: "NextReviewAt",
                schema: "tools",
                table: "UserDictionaryProgress");

            migrationBuilder.DropColumn(
                name: "RepeatLaterCount",
                schema: "tools",
                table: "UserDictionaryProgress");
        }
    }
}
