using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubscriptionToSinglePlan : Migration
    {
        private const string StandardPlanId = "11111111-1111-1111-1111-111111111111";
        private const string PremiumPlanId  = "22222222-2222-2222-2222-222222222222";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Переносим существующих подписчиков Premium на единственный план
            migrationBuilder.Sql($@"
UPDATE payments.""UserSubscriptions""
SET ""SubscriptionPlanId"" = '{StandardPlanId}'::uuid,
    ""PlanName"" = 'Подписка',
    ""Price"" = 29.00,
    ""Currency"" = 'byn'
WHERE ""SubscriptionPlanId"" = '{PremiumPlanId}'::uuid;
");

            // Старый Standard превращаем в новую единственную подписку
            migrationBuilder.Sql($@"
UPDATE payments.""SubscriptionPlans""
SET ""Name"" = 'Подписка',
    ""Description"" = 'Доступ к 2 индивидуальным и 2 групповым живым занятиям в месяц.',
    ""Price"" = 29.00,
    ""Currency"" = 'byn',
    ""IsFeatured"" = true,
    ""SortOrder"" = 1,
    ""IndividualSlotsPerMonth"" = 2,
    ""GroupSlotsPerMonth"" = 2
WHERE ""Id"" = '{StandardPlanId}'::uuid;
");

            // Удаляем Premium-план
            migrationBuilder.Sql($@"
DELETE FROM payments.""SubscriptionPlans"" WHERE ""Id"" = '{PremiumPlanId}'::uuid;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Восстанавливаем старый Standard
            migrationBuilder.Sql($@"
UPDATE payments.""SubscriptionPlans""
SET ""Name"" = 'Standard',
    ""Description"" = 'Доступ к 8 групповым живым занятиям в месяц.',
    ""Price"" = 9.99,
    ""Currency"" = 'usd',
    ""IsFeatured"" = false,
    ""SortOrder"" = 1,
    ""IndividualSlotsPerMonth"" = 0,
    ""GroupSlotsPerMonth"" = 8
WHERE ""Id"" = '{StandardPlanId}'::uuid;
");

            // Возвращаем Premium-план
            migrationBuilder.Sql($@"
INSERT INTO payments.""SubscriptionPlans"" (
    ""Id"", ""Name"", ""Description"", ""Price"", ""Currency"",
    ""BillingInterval"", ""BillingIntervalCount"",
    ""IsActive"", ""IsFeatured"", ""SortOrder"",
    ""ProviderProductId"", ""ProviderPriceId"",
    ""IndividualSlotsPerMonth"", ""GroupSlotsPerMonth"",
    ""CreatedAt""
)
VALUES (
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
    }
}
