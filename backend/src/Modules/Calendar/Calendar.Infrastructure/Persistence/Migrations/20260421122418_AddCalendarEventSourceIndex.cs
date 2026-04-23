using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarEventSourceIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_CourseId",
                schema: "calendar",
                table: "CalendarEvents",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_SourceType_SourceId_UserId",
                schema: "calendar",
                table: "CalendarEvents",
                columns: new[] { "SourceType", "SourceId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_UserId_EventDate",
                schema: "calendar",
                table: "CalendarEvents",
                columns: new[] { "UserId", "EventDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_CourseId",
                schema: "calendar",
                table: "CalendarEvents");

            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_SourceType_SourceId_UserId",
                schema: "calendar",
                table: "CalendarEvents");

            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_UserId_EventDate",
                schema: "calendar",
                table: "CalendarEvents");
        }
    }
}
