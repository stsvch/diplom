// Файл: PlatformSettingsController.cs
using Auth.Application.Commands.UpdatePlatformSettings;
using Auth.Application.DTOs;
using Auth.Application.Queries.GetPlatformSettings;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPlatform.Host.Controllers;

// Контроллер PlatformSettingsController группирует HTTP-эндпоинты и делегирует работу в прикладные сценарии.
[ApiController]
[Route("api/platform-settings")]
public class PlatformSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Публичный эндпоинт возвращает только нечувствительные флаги для landing/auth страниц.
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PlatformSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublic(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPlatformSettingsQuery(), cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "SETTINGS_FAILED"))
            : Ok(result.Value);
    }

    // Административный эндпоинт возвращает полный набор настроек.
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PlatformSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPlatformSettingsQuery(), cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "SETTINGS_FAILED"))
            : Ok(result.Value);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PlatformSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdatePlatformSettingsRequest request, CancellationToken cancellationToken)
    {
        var cmd = new UpdatePlatformSettingsCommand(
            request.RegistrationOpen,
            request.MaintenanceMode,
            request.PlatformName,
            request.SupportEmail);
        var result = await _mediator.Send(cmd, cancellationToken);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "SETTINGS_UPDATE_FAILED"))
            : Ok(result.Value);
    }
}

// API-модель UpdatePlatformSettingsRequest фиксирует тело запроса или результат для действия контроллера.
public record UpdatePlatformSettingsRequest(
    bool RegistrationOpen,
    bool MaintenanceMode,
    string PlatformName,
    string SupportEmail);
