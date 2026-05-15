// Файл: Program.cs
using Auth.Infrastructure.Configuration;
using Auth.Infrastructure.Persistence;
using Calendar.Infrastructure.Configuration;
using Calendar.Infrastructure.Persistence;
using Content.Infrastructure.Configuration;
using Content.Infrastructure.Persistence;
using Courses.Infrastructure.Configuration;
using Courses.Infrastructure.Persistence;
using EduPlatform.Host.Authorization;
using EduPlatform.Host.Middleware;
using EduPlatform.Host.Services;
using EduPlatform.Shared.Application.Behaviors;
using EduPlatform.Shared.Application.Contracts;
using FluentValidation;
using Grading.Infrastructure.Configuration;
using Grading.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
using Payments.Infrastructure.Configuration;
using Payments.Infrastructure.Persistence;
using Scheduling.Infrastructure.Configuration;
using Scheduling.Infrastructure.Persistence;
using Tools.Infrastructure.Configuration;
using Tools.Infrastructure.Persistence;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.Local.json", optional: true, reloadOnChange: true);

// Настройка Serilog.
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Подключение MVC-контроллеров.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // LessonBlockData — полиморфный value object. Кастомный converter:
        //   пишет $type первым свойством, при чтении принимает $type или legacy type.
        options.JsonSerializerOptions.Converters.Add(
            new Content.Infrastructure.Persistence.JsonConverters.LessonBlockDataLenientConverter());
    });

// Подключение Swagger для документации API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация модуля Auth: Identity, JWT, DbContext, MediatR, FluentValidation и AutoMapper.
builder.Services.AddAuthModule(builder.Configuration);

// Регистрация модуля Courses.
builder.Services.AddCoursesModule(builder.Configuration);

// Регистрация модуля Content.
builder.Services.AddContentModule(builder.Configuration);

// Регистрация модуля Tests.
builder.Services.AddTestsModule(builder.Configuration);

// Регистрация модуля Assignments.
builder.Services.AddAssignmentsModule(builder.Configuration);

// Регистрация модуля Grading.
builder.Services.AddGradingModule(builder.Configuration);

// Регистрация модуля Progress.
builder.Services.AddProgressModule(builder.Configuration);

// Регистрация модуля Notifications.
builder.Services.AddNotificationsModule(builder.Configuration);
builder.Services.AddSignalR();

// Регистрация модуля Calendar.
builder.Services.AddCalendarModule(builder.Configuration);

// Регистрация модуля Messaging.
builder.Services.AddMessagingModule(builder.Configuration);

// Регистрация модуля Scheduling.
builder.Services.AddSchedulingModule(builder.Configuration);

// Регистрация модуля Payments.
builder.Services.AddPaymentsModule(builder.Configuration);

// Регистрация модуля Tools.
builder.Services.AddToolsModule(builder.Configuration);

// Общий pipeline MediatR запускает валидацию до обработчиков.
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped<IUserDeletionGuard, UserDeletionGuard>();
builder.Services.AddScoped<LessonAccessService>();
builder.Services.AddScoped<StudentDashboardReadService>();
builder.Services.AddScoped<TeacherDashboardReadService>();
builder.Services.AddScoped<TeacherCourseReportReadService>();
builder.Services.AddScoped<CourseBuilderReadService>();
builder.Services.AddScoped<CourseItemSyncService>();
builder.Services.AddScoped<CourseItemManagementService>();
builder.Services.AddScoped<CourseReviewService>();
builder.Services.AddScoped<AdminAnalyticsReadService>();
builder.Services.AddHostedService<ChatAttachmentCleanupService>();

// Политики авторизации ограничивают доступ к чатам.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, ChatParticipantAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CourseChatOwnerAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ChatParticipant", policy => policy.Requirements.Add(new ChatParticipantRequirement()));
    options.AddPolicy("CourseChatOwner", policy => policy.Requirements.Add(new CourseChatOwnerRequirement()));
});

// Настройка CORS для запросов фронтенда.
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

// Применение миграций и первичное заполнение ролей.
using (var scope = app.Services.CreateScope())
{
    var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await authDb.Database.MigrateAsync();
    await AuthModuleRegistration.SeedRolesAsync(scope.ServiceProvider);
    await AuthModuleRegistration.SeedAdminAsync(scope.ServiceProvider, builder.Configuration);
    await AuthModuleRegistration.SeedTestStudentsAsync(scope.ServiceProvider);
    await AuthModuleRegistration.SeedTestTeachersAsync(scope.ServiceProvider);

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

    var paymentsDb = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    await paymentsDb.Database.MigrateAsync();

    var toolsDb = scope.ServiceProvider.GetRequiredService<ToolsDbContext>();
    await toolsDb.Database.MigrateAsync();
}

// Подключение middleware в конвейер обработки запросов.
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
