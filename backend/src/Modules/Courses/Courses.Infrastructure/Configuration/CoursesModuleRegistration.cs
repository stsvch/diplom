using Courses.Application.Interfaces;
using Courses.Application.Mappings;
using Courses.Domain.Entities;
using Courses.Infrastructure.Persistence;
using Courses.Infrastructure.Services;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Application.Interfaces;
using EduPlatform.Shared.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Courses.Infrastructure.Configuration;

public static class CoursesModuleRegistration
{
    public static IServiceCollection AddCoursesModule(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(CoursesMappingProfile).Assembly;

        // DbContext
        services.AddDbContext<CoursesDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

        services.AddScoped<ICoursesDbContext>(provider => provider.GetRequiredService<CoursesDbContext>());

        // Repositories
        services.AddScoped<IRepository<Discipline>>(provider =>
            new BaseRepository<Discipline>(provider.GetRequiredService<CoursesDbContext>()));
        services.AddScoped<IRepository<Course>>(provider =>
            new BaseRepository<Course>(provider.GetRequiredService<CoursesDbContext>()));
        services.AddScoped<IRepository<CourseModule>>(provider =>
            new BaseRepository<CourseModule>(provider.GetRequiredService<CoursesDbContext>()));
        services.AddScoped<IRepository<Lesson>>(provider =>
            new BaseRepository<Lesson>(provider.GetRequiredService<CoursesDbContext>()));
        services.AddScoped<IRepository<CourseEnrollment>>(provider =>
            new BaseRepository<CourseEnrollment>(provider.GetRequiredService<CoursesDbContext>()));

        services.AddScoped<IEnrollmentReadService, EnrollmentReadService>();
        services.AddScoped<ICoursePaymentReadService, CoursePaymentReadService>();
        services.AddScoped<ICourseAccessProvisioningService, CourseAccessProvisioningService>();
        services.AddScoped<ICourseAccessRevocationService, CourseAccessRevocationService>();

        // MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(applicationAssembly));

        // FluentValidation
        services.AddValidatorsFromAssembly(applicationAssembly);

        // AutoMapper
        services.AddAutoMapper(cfg => { }, applicationAssembly);

        return services;
    }

    public static async Task SeedDisciplinesAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<CoursesDbContext>();

        if (await context.Disciplines.AnyAsync())
            return;

        var defaults = new[]
        {
            new Discipline
            {
                Name = "Английский язык",
                Description = "Изучение грамматики, лексики и разговорной практики английского языка.",
                CreatedAt = DateTime.UtcNow,
            },
            new Discipline
            {
                Name = "Программирование",
                Description = "Основы алгоритмов, языков программирования и разработки ПО.",
                CreatedAt = DateTime.UtcNow,
            },
        };

        context.Disciplines.AddRange(defaults);
        await context.SaveChangesAsync();
    }
}
