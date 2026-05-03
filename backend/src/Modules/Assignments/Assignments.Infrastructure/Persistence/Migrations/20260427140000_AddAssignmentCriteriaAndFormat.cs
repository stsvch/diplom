using System;
using Assignments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignments.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AssignmentsDbContext))]
    [Migration("20260427140000_AddAssignmentCriteriaAndFormat")]
    public partial class AddAssignmentCriteriaAndFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Новая колонка SubmissionFormat
            migrationBuilder.AddColumn<string>(
                name: "SubmissionFormat",
                schema: "assignments",
                table: "Assignments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Both");

            // 2. Новая таблица AssignmentCriteria
            migrationBuilder.CreateTable(
                name: "AssignmentCriteria",
                schema: "assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MaxPoints = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentCriteria_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "assignments",
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentCriteria_AssignmentId",
                schema: "assignments",
                table: "AssignmentCriteria",
                column: "AssignmentId");

            // 3. Backfill: существующая string Criteria → одна запись AssignmentCriteria
            //    с MaxPoints = MaxScore (или 100 если не указано)
            migrationBuilder.Sql("""
                INSERT INTO assignments."AssignmentCriteria"
                    ("Id", "AssignmentId", "Text", "MaxPoints", "OrderIndex")
                SELECT
                    gen_random_uuid(),
                    a."Id",
                    a."Criteria",
                    GREATEST(a."MaxScore", 1),
                    0
                FROM assignments."Assignments" a
                WHERE a."Criteria" IS NOT NULL AND TRIM(a."Criteria") <> '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentCriteria",
                schema: "assignments");

            migrationBuilder.DropColumn(
                name: "SubmissionFormat",
                schema: "assignments",
                table: "Assignments");
        }
    }
}
