using Calendar.Application.Interfaces;
using Calendar.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Calendar.Infrastructure.Configuration;

public static class CalendarModuleRegistration
{
    public static IServiceCollection AddCalendarModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        services.AddDbContext<CalendarDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<ICalendarDbContext>(sp => sp.GetRequiredService<CalendarDbContext>());

        var applicationAssembly = typeof(ICalendarDbContext).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddAutoMapper(cfg => { }, applicationAssembly);

        return services;
    }
}
