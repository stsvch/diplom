// Файл: ExceptionHandlingMiddleware.cs
using System.Net;
using System.Text.Json;
using EduPlatform.Shared.Application.Models;
using FluentValidation;

namespace EduPlatform.Host.Middleware;

// Middleware ExceptionHandlingMiddleware выполняет поперечную обработку HTTP-запросов в конвейере Host.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed: {Errors}", ex.Errors);
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest,
                ApiError.FromValidation(
                    ex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            var apiError = ApiError.FromMessage("Произошла непредвиденная ошибка.", "SERVER_ERROR");

            // В development-режиме отдаём подробности — это очень помогает при отладке.
            // В production они скрываются за generic-сообщением.
            if (_env.IsDevelopment())
            {
                var inner = ex.InnerException;
                apiError = new ApiError
                {
                    Message = "Произошла непредвиденная ошибка.",
                    Code = "SERVER_ERROR",
                    Errors = new Dictionary<string, string[]>
                    {
                        ["exception"] = [$"{ex.GetType().Name}: {ex.Message}"],
                        ["stackTrace"] = (ex.StackTrace ?? string.Empty).Split('\n').Take(15).ToArray(),
                        ["inner"] = inner is null
                            ? Array.Empty<string>()
                            : [$"{inner.GetType().Name}: {inner.Message}"],
                    }
                };
            }

            await WriteErrorResponseAsync(context, HttpStatusCode.InternalServerError, apiError);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode, ApiError error)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(error, JsonOptions));
    }
}
