// TestsModuleRegistration.cs

using EduPlatform.Shared.Application.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tests.Application.Interfaces;
using Tests.Application.Mappings;
using Tests.Infrastructure.Persistence;
using Tests.Infrastructure.Services;

namespace Tests.Infrastructure.Configuration;

/// <summary>
/// Расширение DI регистрирует DbContext, MediatR, валидаторы, маппинг и сервисы модуля.
/// </summary>
public static class TestsModuleRegistration
{
    public static IServiceCollection AddTestsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(TestsMappingProfile).Assembly;

        // Контекст EF Core.
        services.AddDbContext<TestsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

        services.AddScoped<ITestsDbContext>(provider => provider.GetRequiredService<TestsDbContext>());
        services.AddScoped<ITestReadService, TestReadService>();

        // Регистрация обработчиков MediatR.
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(applicationAssembly));

        // Регистрация валидаторов FluentValidation.
        services.AddValidatorsFromAssembly(applicationAssembly);

        // Регистрация профилей AutoMapper.
        services.AddAutoMapper(cfg => { }, applicationAssembly);

        return services;
    }
}
