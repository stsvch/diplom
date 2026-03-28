using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tests.Application.Interfaces;
using Tests.Application.Mappings;
using Tests.Infrastructure.Persistence;

namespace Tests.Infrastructure.Configuration;

public static class TestsModuleRegistration
{
    public static IServiceCollection AddTestsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(TestsMappingProfile).Assembly;

        // DbContext
        services.AddDbContext<TestsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

        services.AddScoped<ITestsDbContext>(provider => provider.GetRequiredService<TestsDbContext>());

        // MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(applicationAssembly));

        // FluentValidation
        services.AddValidatorsFromAssembly(applicationAssembly);

        // AutoMapper
        services.AddAutoMapper(cfg => { }, applicationAssembly);

        return services;
    }
}
