using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "payments");

            migrationBuilder.CreateTable(
                name: "CoursePurchases",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TeacherId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    StudentId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    PaymentAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePurchases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAttempts",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TeacherId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    StudentId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    StudentName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SavePaymentMethodRequested = table.Column<bool>(type: "boolean", nullable: false),
                    ProviderCustomerId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderSessionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderPaymentIntentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderPaymentMethodId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FailureCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderCustomerId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProviderPaymentMethodId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Last4 = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    ExpMonth = table.Column<int>(type: "integer", nullable: true),
                    ExpYear = table.Column<int>(type: "integer", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedWebhookEvents",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderEventId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedWebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeacherPayoutAccounts",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderAccountId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChargesEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PayoutsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DetailsSubmitted = table.Column<bool>(type: "boolean", nullable: false),
                    RequirementsSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OnboardingStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadyAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherPayoutAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPaymentProfiles",
                schema: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProviderCustomerId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPaymentProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoursePurchases_StudentId_CourseId",
                schema: "payments",
                table: "CoursePurchases",
                columns: new[] { "StudentId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_ProviderPaymentIntentId",
                schema: "payments",
                table: "PaymentAttempts",
                column: "ProviderPaymentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_ProviderSessionId",
                schema: "payments",
                table: "PaymentAttempts",
                column: "ProviderSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_StudentId_CreatedAt",
                schema: "payments",
                table: "PaymentAttempts",
                columns: new[] { "StudentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ProviderPaymentMethodId",
                schema: "payments",
                table: "PaymentMethods",
                column: "ProviderPaymentMethodId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_UserId_IsDefault",
                schema: "payments",
                table: "PaymentMethods",
                columns: new[] { "UserId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvents_Provider_ProviderEventId",
                schema: "payments",
                table: "ProcessedWebhookEvents",
                columns: new[] { "Provider", "ProviderEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherPayoutAccounts_ProviderAccountId",
                schema: "payments",
                table: "TeacherPayoutAccounts",
                column: "ProviderAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherPayoutAccounts_TeacherId",
                schema: "payments",
                table: "TeacherPayoutAccounts",
                column: "TeacherId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPaymentProfiles_ProviderCustomerId",
                schema: "payments",
                table: "UserPaymentProfiles",
                column: "ProviderCustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPaymentProfiles_UserId",
                schema: "payments",
                table: "UserPaymentProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoursePurchases",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "PaymentAttempts",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "PaymentMethods",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "ProcessedWebhookEvents",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "TeacherPayoutAccounts",
                schema: "payments");

            migrationBuilder.DropTable(
                name: "UserPaymentProfiles",
                schema: "payments");
        }
    }
}
