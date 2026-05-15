using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAssignmentCriteriaPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPoints",
                schema: "assignments",
                table: "AssignmentCriteria");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxPoints",
                schema: "assignments",
                table: "AssignmentCriteria",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
