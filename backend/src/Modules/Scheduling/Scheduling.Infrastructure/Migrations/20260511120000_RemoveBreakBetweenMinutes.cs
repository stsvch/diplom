using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBreakBetweenMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakBetweenMinutes",
                schema: "scheduling",
                table: "TeacherAvailabilities");

            migrationBuilder.DropColumn(
                name: "SlotDurationMinutes",
                schema: "scheduling",
                table: "TeacherAvailabilities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BreakBetweenMinutes",
                schema: "scheduling",
                table: "TeacherAvailabilities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SlotDurationMinutes",
                schema: "scheduling",
                table: "TeacherAvailabilities",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
