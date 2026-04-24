using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProviderPayoutTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayoutRecords_ProviderPayoutId",
                schema: "payments",
                table: "PayoutRecords");

            migrationBuilder.DropColumn(
                name: "ProviderPayoutId",
                schema: "payments",
                table: "PayoutRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderPayoutId",
                schema: "payments",
                table: "PayoutRecords",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRecords_ProviderPayoutId",
                schema: "payments",
                table: "PayoutRecords",
                column: "ProviderPayoutId");
        }
    }
}
