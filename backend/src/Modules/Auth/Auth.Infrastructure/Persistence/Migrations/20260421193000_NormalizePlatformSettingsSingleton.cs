using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Infrastructure.Persistence.Migrations
{
    public partial class NormalizePlatformSettingsSingleton : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    keeper_id integer;
                BEGIN
                    SELECT "Id"
                    INTO keeper_id
                    FROM "PlatformSettings"
                    ORDER BY "Id"
                    LIMIT 1;

                    IF keeper_id IS NULL THEN
                        INSERT INTO "PlatformSettings" ("Id", "RegistrationOpen", "MaintenanceMode", "PlatformName", "SupportEmail", "UpdatedAt")
                        VALUES (1, TRUE, FALSE, 'EduPlatform', 'support@eduplatform.local', NOW());
                    ELSE
                        DELETE FROM "PlatformSettings"
                        WHERE "Id" <> keeper_id;

                        IF keeper_id <> 1 THEN
                            UPDATE "PlatformSettings"
                            SET "Id" = 1
                            WHERE "Id" = keeper_id;
                        END IF;
                    END IF;
                END $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
