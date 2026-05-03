using Courses.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Courses.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(CoursesDbContext))]
    [Migration("20260426160000_AddCourseItems")]
    public partial class AddCourseItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseItems",
                schema: "courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResourceKind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Points = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AvailableFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseItems_CourseModules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "courses",
                        principalTable: "CourseModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CourseItems_Courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "courses",
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseItems_CourseId",
                schema: "courses",
                table: "CourseItems",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseItems_CourseId_ModuleId_OrderIndex",
                schema: "courses",
                table: "CourseItems",
                columns: new[] { "CourseId", "ModuleId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseItems_ModuleId",
                schema: "courses",
                table: "CourseItems",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseItems_Type_SourceId",
                schema: "courses",
                table: "CourseItems",
                columns: new[] { "Type", "SourceId" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO courses."CourseItems" (
                    "Id",
                    "CourseId",
                    "ModuleId",
                    "Type",
                    "SourceId",
                    "Title",
                    "Description",
                    "OrderIndex",
                    "Status",
                    "IsRequired",
                    "CreatedAt"
                )
                SELECT
                    l."Id",
                    m."CourseId",
                    l."ModuleId",
                    'Lesson',
                    l."Id",
                    l."Title",
                    l."Description",
                    l."OrderIndex",
                    CASE WHEN l."IsPublished" THEN 'Published' ELSE 'Draft' END,
                    TRUE,
                    timezone('utc', now())
                FROM courses."Lessons" l
                INNER JOIN courses."CourseModules" m ON m."Id" = l."ModuleId"
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM courses."CourseItems" ci
                    WHERE ci."Type" = 'Lesson' AND ci."SourceId" = l."Id"
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseItems",
                schema: "courses");
        }
    }
}
