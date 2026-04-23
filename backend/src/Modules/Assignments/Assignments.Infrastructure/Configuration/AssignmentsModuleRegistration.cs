using Assignments.Application.Interfaces;
using Assignments.Infrastructure.Persistence;
using Assignments.Infrastructure.Services;
using EduPlatform.Shared.Application.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Assignments.Infrastructure.Configuration;

public static class AssignmentsModuleRegistration
{
    public static IServiceCollection AddAssignmentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL");
        services.AddDbContext<AssignmentsDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IAssignmentsDbContext>(sp => sp.GetRequiredService<AssignmentsDbContext>());
        services.AddScoped<IAssignmentReadService, AssignmentReadService>();

        var applicationAssembly = typeof(IAssignmentsDbContext).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);
        services.AddAutoMapper(cfg => { }, applicationAssembly);

        return services;
    }
}
