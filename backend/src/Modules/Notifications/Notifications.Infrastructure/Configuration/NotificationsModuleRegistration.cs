using EduPlatform.Shared.Application.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application.Interfaces;
using Notifications.Infrastructure.Persistence;
using Notifications.Infrastructure.Services;

namespace Notifications.Infrastructure.Configuration;

public static class NotificationsModuleRegistration
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<INotificationsDbContext>(sp => sp.GetRequiredService<NotificationsDbContext>());
        services.AddScoped<INotificationSender, SignalRNotificationSender>();
        services.AddScoped<INotificationDispatcher, NotificationPublisher>();

        var applicationAssembly = typeof(INotificationsDbContext).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddAutoMapper(cfg => { }, applicationAssembly);

        return services;
    }
}
