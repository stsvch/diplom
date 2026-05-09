using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tools.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGlossaryNoteImageOptionalTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Translation",
                schema: "tools",
                table: "DictionaryWords",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "ImageStorageKey",
                schema: "tools",
                table: "DictionaryWords",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "tools",
                table: "DictionaryWords",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageStorageKey",
                schema: "tools",
                table: "DictionaryWords");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "tools",
                table: "DictionaryWords");

            migrationBuilder.AlterColumn<string>(
                name: "Translation",
                schema: "tools",
                table: "DictionaryWords",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
