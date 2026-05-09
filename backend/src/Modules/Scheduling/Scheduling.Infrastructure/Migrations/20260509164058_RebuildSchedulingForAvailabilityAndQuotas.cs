using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RebuildSchedulingForAvailabilityAndQuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SessionBookings_SlotId",
                schema: "scheduling",
                table: "SessionBookings");

            migrationBuilder.DropColumn(
                name: "CourseName",
                schema: "scheduling",
                table: "ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "IsGroupSession",
                schema: "scheduling",
                table: "ScheduleSlots");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                schema: "scheduling",
                table: "ScheduleSlots",
                newName: "RequiredCourseId");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                schema: "scheduling",
                table: "SessionBookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                schema: "scheduling",
                table: "SessionBookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionUsageId",
                schema: "scheduling",
                table: "SessionBookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "scheduling",
                table: "ScheduleSlots",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AvailabilityId",
                schema: "scheduling",
                table: "ScheduleSlots",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionType",
                schema: "scheduling",
                table: "ScheduleSlots",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Individual");

            migrationBuilder.CreateTable(
                name: "TeacherAvailabilities",
                schema: "scheduling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TeacherName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DayOfWeek = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    SpecificDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    SlotDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    BreakBetweenMinutes = table.Column<int>(type: "integer", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    SessionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MaxStudents = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MeetingLink = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RequiredCourseId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAvailabilities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_SlotId_StudentId",
                schema: "scheduling",
                table: "SessionBookings",
                columns: new[] { "SlotId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_StudentId_StartTime",
                schema: "scheduling",
                table: "SessionBookings",
                columns: new[] { "StudentId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_AvailabilityId_StartTime",
                schema: "scheduling",
                table: "ScheduleSlots",
                columns: new[] { "AvailabilityId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_TeacherId_StartTime",
                schema: "scheduling",
                table: "ScheduleSlots",
                columns: new[] { "TeacherId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAvailabilities_RequiredCourseId_IsActive",
                schema: "scheduling",
                table: "TeacherAvailabilities",
                columns: new[] { "RequiredCourseId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAvailabilities_TeacherId_IsActive",
                schema: "scheduling",
                table: "TeacherAvailabilities",
                columns: new[] { "TeacherId", "IsActive" });

            // Расширение для GiST-индексов по диапазонам времени
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS btree_gist;");

            // Защита от пересечений у учителя: один слот в одно время
            migrationBuilder.Sql(@"
ALTER TABLE scheduling.""ScheduleSlots""
ADD CONSTRAINT ""CK_ScheduleSlots_NoTeacherOverlap""
EXCLUDE USING gist (
    ""TeacherId"" WITH =,
    tstzrange(""StartTime"", ""EndTime"", '[)') WITH &&
)
WHERE (""Status"" IN ('Available', 'Booked', 'Full'));");

            // Защита от пересечений у студента: одна бронь в одно время
            migrationBuilder.Sql(@"
ALTER TABLE scheduling.""SessionBookings""
ADD CONSTRAINT ""CK_SessionBookings_NoStudentOverlap""
EXCLUDE USING gist (
    ""StudentId"" WITH =,
    tstzrange(""StartTime"", ""EndTime"", '[)') WITH &&
)
WHERE (""Status"" = 'Booked');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE scheduling.""SessionBookings"" DROP CONSTRAINT IF EXISTS ""CK_SessionBookings_NoStudentOverlap"";");
            migrationBuilder.Sql(@"ALTER TABLE scheduling.""ScheduleSlots"" DROP CONSTRAINT IF EXISTS ""CK_ScheduleSlots_NoTeacherOverlap"";");

            migrationBuilder.DropTable(
                name: "TeacherAvailabilities",
                schema: "scheduling");

            migrationBuilder.DropIndex(
                name: "IX_SessionBookings_SlotId_StudentId",
                schema: "scheduling",
                table: "SessionBookings");

            migrationBuilder.DropIndex(
                name: "IX_SessionBookings_StudentId_StartTime",
                schema: "scheduling",
                table: "SessionBookings");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleSlots_AvailabilityId_StartTime",
                schema: "scheduling",
                table: "ScheduleSlots");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleSlots_TeacherId_StartTime",
                schema: "scheduling",
                table: "ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "EndTime",
                schema: "scheduling",
                table: "SessionBookings");

            migrationBuilder.DropColumn(
                name: "StartTime",
                schema: "scheduling",
                table: "SessionBookings");

            migrationBuilder.DropColumn(
                name: "SubscriptionUsageId",
                schema: "scheduling",
                table: "SessionBookings");

            migrationBuilder.DropColumn(
                name: "AvailabilityId",
                schema: "scheduling",
                table: "ScheduleSlots");

            migrationBuilder.DropColumn(
                name: "SessionType",
                schema: "scheduling",
                table: "ScheduleSlots");

            migrationBuilder.RenameColumn(
                name: "RequiredCourseId",
                schema: "scheduling",
                table: "ScheduleSlots",
                newName: "CourseId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "scheduling",
                table: "ScheduleSlots",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourseName",
                schema: "scheduling",
                table: "ScheduleSlots",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGroupSession",
                schema: "scheduling",
                table: "ScheduleSlots",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_SlotId",
                schema: "scheduling",
                table: "SessionBookings",
                column: "SlotId");
        }
    }
}
