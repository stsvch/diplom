using Courses.Application.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tools.Application.Interfaces;
using Tools.Infrastructure.Persistence;
using Tools.Infrastructure.Services;

namespace Tools.Infrastructure.Configuration;

public static class ToolsModuleRegistration
{
    public static IServiceCollection AddToolsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");

        services.AddDbContext<ToolsDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IToolsDbContext>(sp => sp.GetRequiredService<ToolsDbContext>());

        services.AddScoped<GlossaryService>();
        services.AddScoped<IGlossaryService>(sp => sp.GetRequiredService<GlossaryService>());

        var applicationAssembly = typeof(IGlossaryService).Assembly;
        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddAutoMapper(cfg => { }, applicationAssembly);

        return services;
    }
}
