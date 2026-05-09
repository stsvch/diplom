using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSubscriptionPlans : Migration
    {
        private const string StandardPlanId = "11111111-1111-1111-1111-111111111111";
        private const string PremiumPlanId  = "22222222-2222-2222-2222-222222222222";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
INSERT INTO payments.""SubscriptionPlans"" (
    ""Id"", ""Name"", ""Description"", ""Price"", ""Currency"",
    ""BillingInterval"", ""BillingIntervalCount"",
    ""IsActive"", ""IsFeatured"", ""SortOrder"",
    ""ProviderProductId"", ""ProviderPriceId"",
    ""IndividualSlotsPerMonth"", ""GroupSlotsPerMonth"",
    ""CreatedAt""
)
VALUES
    (
        '{StandardPlanId}'::uuid, 'Standard', 'Доступ к 8 групповым живым занятиям в месяц.',
        9.99, 'usd', 'Month', 1,
        true, false, 1,
        NULL, NULL,
        0, 8,
        NOW() AT TIME ZONE 'UTC'
    ),
    (
        '{PremiumPlanId}'::uuid, 'Premium', 'Доступ к 8 групповым и 8 индивидуальным живым занятиям в месяц.',
        19.99, 'usd', 'Month', 1,
        true, true, 2,
        NULL, NULL,
        8, 8,
        NOW() AT TIME ZONE 'UTC'
    )
ON CONFLICT (""Id"") DO NOTHING;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DELETE FROM payments.""SubscriptionPlans""
WHERE ""Id"" IN ('{StandardPlanId}'::uuid, '{PremiumPlanId}'::uuid);
");
        }
    }
}
