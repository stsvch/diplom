using EduPlatform.Shared.Application.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application.Interfaces;
using Payments.Infrastructure.Persistence;
using Payments.Infrastructure.Services;

namespace Payments.Infrastructure.Configuration;

public static class PaymentsModuleRegistration
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        services.Configure<PaymentsOptions>(options =>
        {
            options.Provider = configuration["Payments:Provider"] ?? "Stripe";
            options.Currency = configuration["Payments:Currency"] ?? "usd";
            options.SettlementHoldDays = int.TryParse(configuration["Payments:SettlementHoldDays"], out var holdDays)
                ? Math.Max(0, holdDays)
                : 7;
            options.PlatformCommissionPercent = decimal.TryParse(configuration["Payments:PlatformCommissionPercent"], out var commission)
                ? Math.Max(0, commission)
                : 0m;
        });
        services.Configure<StripeOptions>(options =>
        {
            options.SecretKey = configuration["Stripe:SecretKey"] ?? string.Empty;
            options.WebhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty;
            options.Country = configuration["Stripe:Country"] ?? "US";
        });

        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IPaymentsDbContext>(sp => sp.GetRequiredService<PaymentsDbContext>());

        services.AddScoped<HttpClient>();
        services.AddScoped<IPaymentProviderGateway, StripePaymentGateway>();
        services.AddScoped<PaymentsService>();
        services.AddScoped<IPaymentsService>(sp => sp.GetRequiredService<PaymentsService>());
        services.AddScoped<ITeacherPayoutReadService>(sp => sp.GetRequiredService<PaymentsService>());

        var applicationAssembly = typeof(IPaymentsService).Assembly;
        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddAutoMapper(cfg => { }, applicationAssembly);

        return services;
    }
}
