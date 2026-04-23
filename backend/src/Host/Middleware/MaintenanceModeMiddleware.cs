using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using EduPlatform.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EduPlatform.Host.Middleware;

public class MaintenanceModeMiddleware
{
    private static readonly string[] AllowlistPrefixes =
    {
        "/api/admin",
        "/api/users/me",
        "/api/auth/login",
        "/api/auth/refresh",
        "/api/platform-settings",
        "/hubs",
    };

    private readonly RequestDelegate _next;

    public MaintenanceModeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthDbContext db)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Non-API paths pass through (static, swagger, etc.)
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Admin bypass via role claim
        var isAdmin = context.User?.FindAll(ClaimTypes.Role).Any(c => c.Value == "Admin") == true;
        if (isAdmin)
        {
            await _next(context);
            return;
        }

        // Allowlist prefixes
        foreach (var prefix in AllowlistPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }
        }

        var settings = await db.PlatformSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == PlatformSetting.SingletonId);
        if (settings?.MaintenanceMode == true)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";
            var error = ApiError.FromMessage("Платформа на техническом обслуживании. Попробуйте позже.", "MAINTENANCE_MODE");
            await context.Response.WriteAsync(JsonSerializer.Serialize(error));
            return;
        }

        await _next(context);
    }
}
