using Courses.Application.Interfaces;
using Courses.Application.Mappings;
using Courses.Domain.Entities;
using Courses.Infrastructure.Persistence;
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
        services.AddScoped<IRepository<LessonBlock>>(provider =>
            new BaseRepository<LessonBlock>(provider.GetRequiredService<CoursesDbContext>()));
        services.AddScoped<IRepository<CourseEnrollment>>(provider =>
            new BaseRepository<CourseEnrollment>(provider.GetRequiredService<CoursesDbContext>()));

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
