using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderTransferIdToPayoutRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderTransferId",
                schema: "payments",
                table: "PayoutRecords",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRecords_ProviderTransferId",
                schema: "payments",
                table: "PayoutRecords",
                column: "ProviderTransferId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayoutRecords_ProviderTransferId",
                schema: "payments",
                table: "PayoutRecords");

            migrationBuilder.DropColumn(
                name: "ProviderTransferId",
                schema: "payments",
                table: "PayoutRecords");
        }
    }
}
