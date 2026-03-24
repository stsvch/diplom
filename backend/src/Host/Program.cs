using Auth.Infrastructure.Configuration;
using Auth.Infrastructure.Persistence;
using Content.Infrastructure.Configuration;
using Content.Infrastructure.Persistence;
using Courses.Infrastructure.Configuration;
using Courses.Infrastructure.Persistence;
using EduPlatform.Host.Middleware;
using EduPlatform.Shared.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

// Shared MediatR pipeline behavior (validation)
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

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

    var coursesDb = scope.ServiceProvider.GetRequiredService<CoursesDbContext>();
    await coursesDb.Database.MigrateAsync();

    var contentDb = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
    await contentDb.Database.MigrateAsync();
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
app.MapControllers();

app.Run();
