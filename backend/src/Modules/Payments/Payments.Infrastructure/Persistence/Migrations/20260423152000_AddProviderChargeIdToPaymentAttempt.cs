using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderChargeIdToPaymentAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderChargeId",
                schema: "payments",
                table: "PaymentAttempts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_ProviderChargeId",
                schema: "payments",
                table: "PaymentAttempts",
                column: "ProviderChargeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentAttempts_ProviderChargeId",
                schema: "payments",
                table: "PaymentAttempts");

            migrationBuilder.DropColumn(
                name: "ProviderChargeId",
                schema: "payments",
                table: "PaymentAttempts");
        }
    }
}
