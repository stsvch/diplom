using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendar.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Удаляет события календаря типа Workshop, у которых исходный слот не имеет активных броней.
    /// До этой миграции событие учителя создавалось при любом UpdateSlot, даже если никто не записался.
    /// </summary>
    public partial class CleanupOrphanWorkshopEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM calendar.""CalendarEvents"" ce
WHERE ce.""Type"" = 'Workshop'
  AND ce.""SourceType"" = 'ScheduleSlot'
  AND ce.""SourceId"" IS NOT NULL
  AND NOT EXISTS (
    SELECT 1
    FROM scheduling.""SessionBookings"" sb
    WHERE sb.""SlotId"" = ce.""SourceId""
      AND sb.""Status"" = 'Booked'
  );
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Восстановить удалённые события невозможно без снимка — оставляем no-op.
        }
    }
}
