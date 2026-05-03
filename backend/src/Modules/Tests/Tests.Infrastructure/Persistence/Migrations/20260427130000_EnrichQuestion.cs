using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Tests.Infrastructure.Persistence;

#nullable disable

namespace Tests.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(TestsDbContext))]
    [Migration("20260427130000_EnrichQuestion")]
    public partial class EnrichQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GradeType",
                schema: "tests",
                table: "Questions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Auto");

            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                schema: "tests",
                table: "Questions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedAnswer",
                schema: "tests",
                table: "Questions",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: true);

            // Backfill: вопросы типа OpenAnswer/TextInput → Manual
            migrationBuilder.Sql("""
                UPDATE tests."Questions"
                SET "GradeType" = 'Manual'
                WHERE "Type" IN ('OpenAnswer', 'TextInput');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ExpectedAnswer", schema: "tests", table: "Questions");
            migrationBuilder.DropColumn(name: "Explanation", schema: "tests", table: "Questions");
            migrationBuilder.DropColumn(name: "GradeType", schema: "tests", table: "Questions");
        }
    }
}
