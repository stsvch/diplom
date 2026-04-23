using EduPlatform.Shared.Application.Contracts;
using Grading.Application.Interfaces;
using Grading.Infrastructure.Persistence;
using Grading.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace Grading.Infrastructure.Configuration;

public static class GradingModuleRegistration
{
    public static IServiceCollection AddGradingModule(this IServiceCollection services, IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var connectionString = configuration.GetConnectionString("PostgreSQL");
        services.AddDbContext<GradingDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IGradingDbContext>(sp => sp.GetRequiredService<GradingDbContext>());

        services.AddScoped<IExportService, ExcelExportService>();
        services.AddScoped<PdfExportService>();
        services.AddScoped<IGradeRecordWriter, GradeRecordWriter>();

        var applicationAssembly = typeof(IGradingDbContext).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

        return services;
    }
}
