// ContentModuleRegistration.cs

using Content.Application.CodeExecution;
using Content.Application.Grading;
using Content.Application.Grading.Graders;
using Content.Application.Interfaces;
using Content.Application.Mappings;
using Content.Application.Validation;
using Content.Application.Validation.Validators;
using Content.Infrastructure.Persistence;
using Content.Infrastructure.Services;
using EduPlatform.Shared.Application.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace Content.Infrastructure.Configuration;

// DI-регистрация class: подключает инфраструктуру, сервисы и application-компоненты модуля.
public static class ContentModuleRegistration
{
    public static IServiceCollection AddContentModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(ContentMappingProfile).Assembly;

        // Регистрация DbContext.
        services.AddDbContext<ContentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

        services.AddScoped<IContentDbContext>(provider => provider.GetRequiredService<ContentDbContext>());

        // Настройка клиента MinIO.
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

        // Регистрация сервисов.
        services.AddScoped<IFileStorageService, MinioFileStorageService>();
        services.AddScoped<ILessonContentCleaner, LessonContentCleaner>();
        services.AddScoped<IContentReadService, ContentReadService>();
        services.AddScoped<IAttachmentCleaner, AttachmentCleaner>();
        services.AddSingleton<ICodeExecutor, ProcessCodeExecutor>();

        // Регистрация проверяющих блоков.
        services.AddScoped<IBlockGrader, SingleChoiceGrader>();
        services.AddScoped<IBlockGrader, MultipleChoiceGrader>();
        services.AddScoped<IBlockGrader, TrueFalseGrader>();
        services.AddScoped<IBlockGrader, FillGapGrader>();
        services.AddScoped<IBlockGrader, DropdownGrader>();
        services.AddScoped<IBlockGrader, WordBankGrader>();
        services.AddScoped<IBlockGrader, ReorderGrader>();
        services.AddScoped<IBlockGrader, MatchingGrader>();
        services.AddScoped<IBlockGrader, OpenTextGrader>();
        services.AddScoped<IBlockGrader, CodeExerciseGrader>();
        services.AddScoped<IBlockGraderRegistry, BlockGraderRegistry>();

        // Регистрация валидаторов данных блоков.
        services.AddScoped<IBlockDataValidator, TextBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, VideoBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, AudioBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, ImageBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, BannerBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, FileBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, SingleChoiceBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, MultipleChoiceBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, TrueFalseBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, FillGapBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, DropdownBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, WordBankBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, ReorderBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, MatchingBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, OpenTextBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, CodeExerciseBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, QuizBlockDataValidator>();
        services.AddScoped<IBlockDataValidator, AssignmentBlockDataValidator>();
        services.AddScoped<IBlockDataValidatorRegistry, BlockDataValidatorRegistry>();

        // Регистрация MediatR.
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(applicationAssembly));

        // Регистрация FluentValidation.
        services.AddValidatorsFromAssembly(applicationAssembly);

        // Регистрация AutoMapper.
        services.AddAutoMapper(cfg => { }, applicationAssembly);

        return services;
    }
}
