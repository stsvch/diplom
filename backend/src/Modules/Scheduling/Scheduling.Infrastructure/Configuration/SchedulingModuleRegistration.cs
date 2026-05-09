using EduPlatform.Shared.Application.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.Application.Interfaces;
using Scheduling.Infrastructure.Persistence;
using Scheduling.Infrastructure.Services;

namespace Scheduling.Infrastructure.Configuration;

public static class SchedulingModuleRegistration
{
    public static IServiceCollection AddSchedulingModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        services.AddDbContext<SchedulingDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<ISchedulingDbContext>(sp => sp.GetRequiredService<SchedulingDbContext>());

        // Реальная реализация для модуля Payments (заменяет no-op fallback из Payments).
        services.AddScoped<ICompletedBookingReadService, CompletedBookingReadService>();

        var applicationAssembly = typeof(ISchedulingDbContext).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddAutoMapper(cfg => { }, applicationAssembly);

        return services;
    }
}
