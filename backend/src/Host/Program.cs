using Auth.Infrastructure.Configuration;
using Auth.Infrastructure.Persistence;
using Calendar.Infrastructure.Configuration;
using Calendar.Infrastructure.Persistence;
using Content.Infrastructure.Configuration;
using Content.Infrastructure.Persistence;
using Courses.Infrastructure.Configuration;
using Courses.Infrastructure.Persistence;
using EduPlatform.Host.Middleware;
using EduPlatform.Host.Services;
using EduPlatform.Shared.Application.Behaviors;
using EduPlatform.Shared.Application.Contracts;
using FluentValidation;
using Grading.Infrastructure.Configuration;
using Grading.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notifications.Infrastructure.Configuration;
using Notifications.Infrastructure.Hubs;
using Notifications.Infrastructure.Persistence;
using Progress.Infrastructure.Configuration;
using Progress.Infrastructure.Persistence;
using QuestPDF.Infrastructure;
using Serilog;
using Tests.Infrastructure.Configuration;
using Tests.Infrastructure.Persistence;
using Assignments.Infrastructure.Configuration;
using Assignments.Infrastructure.Persistence;
using Messaging.Infrastructure.Configuration;
using Messaging.Infrastructure.Hubs;
using Scheduling.Infrastructure.Configuration;
using Scheduling.Infrastructure.Persistence;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Auth Module (registers Identity, JWT, DbContext, MediatR handlers, FluentValidation, AutoMapper)
builder.Services.AddAuthModule(builder.Configuration);

// Courses Module
builder.Services.AddCoursesModule(builder.Configuration);

// Content Module
builder.Services.AddContentModule(builder.Configuration);

// Tests Module
builder.Services.AddTestsModule(builder.Configuration);

// Assignments Module
builder.Services.AddAssignmentsModule(builder.Configuration);

// Grading Module
builder.Services.AddGradingModule(builder.Configuration);

// Progress Module
builder.Services.AddProgressModule(builder.Configuration);

// Notifications Module
builder.Services.AddNotificationsModule(builder.Configuration);
builder.Services.AddSignalR();

// Calendar Module
builder.Services.AddCalendarModule(builder.Configuration);

// Messaging Module
builder.Services.AddMessagingModule(builder.Configuration);

// Scheduling Module
builder.Services.AddSchedulingModule(builder.Configuration);

// Shared MediatR pipeline behavior (validation)
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped<IUserDeletionGuard, UserDeletionGuard>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Apply migrations and seed roles
using (var scope = app.Services.CreateScope())
{
    var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await authDb.Database.MigrateAsync();
    await AuthModuleRegistration.SeedRolesAsync(scope.ServiceProvider);
    await AuthModuleRegistration.SeedAdminAsync(scope.ServiceProvider, builder.Configuration);
    await AuthModuleRegistration.SeedTestStudentsAsync(scope.ServiceProvider);

    var coursesDb = scope.ServiceProvider.GetRequiredService<CoursesDbContext>();
    await coursesDb.Database.MigrateAsync();
    await CoursesModuleRegistration.SeedDisciplinesAsync(scope.ServiceProvider);

    var contentDb = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
    await contentDb.Database.MigrateAsync();

    var testsDb = scope.ServiceProvider.GetRequiredService<TestsDbContext>();
    await testsDb.Database.MigrateAsync();

    var assignmentsDb = scope.ServiceProvider.GetRequiredService<AssignmentsDbContext>();
    await assignmentsDb.Database.MigrateAsync();

    var gradingDb = scope.ServiceProvider.GetRequiredService<GradingDbContext>();
    await gradingDb.Database.MigrateAsync();

    var progressDb = scope.ServiceProvider.GetRequiredService<ProgressDbContext>();
    await progressDb.Database.MigrateAsync();

    var notificationsDb = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
    await notificationsDb.Database.MigrateAsync();

    var calendarDb = scope.ServiceProvider.GetRequiredService<CalendarDbContext>();
    await calendarDb.Database.MigrateAsync();

    var schedulingDb = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
    await schedulingDb.Database.MigrateAsync();
}

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<MaintenanceModeMiddleware>();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
