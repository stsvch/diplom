using Content.Application.Interfaces;
using Content.Application.Mappings;
using Content.Infrastructure.Persistence;
using Content.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace Content.Infrastructure.Configuration;

public static class ContentModuleRegistration
{
    public static IServiceCollection AddContentModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(ContentMappingProfile).Assembly;

        // DbContext
        services.AddDbContext<ContentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

        services.AddScoped<IContentDbContext>(provider => provider.GetRequiredService<ContentDbContext>());

        // MinIO client
        var endpoint = configuration["MinIO:Endpoint"] ?? "localhost:9000";
        var accessKey = configuration["MinIO:AccessKey"] ?? "minioadmin";
        var secretKey = configuration["MinIO:SecretKey"] ?? "minioadmin";
        var useSSL = bool.Parse(configuration["MinIO:UseSSL"] ?? "false");

        services.AddSingleton<IMinioClient>(_ =>
            new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(useSSL)
                .Build());

        // Services
        services.AddScoped<IFileStorageService, MinioFileStorageService>();

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
