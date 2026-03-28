using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Progress.Application.Interfaces;
using Progress.Infrastructure.Persistence;

namespace Progress.Infrastructure.Configuration;

public static class ProgressModuleRegistration
{
    public static IServiceCollection AddProgressModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        services.AddDbContext<ProgressDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IProgressDbContext>(sp => sp.GetRequiredService<ProgressDbContext>());

        var applicationAssembly = typeof(IProgressDbContext).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

        return services;
    }
}
