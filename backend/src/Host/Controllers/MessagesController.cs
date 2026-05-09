using EduPlatform.Shared.Application.Models;
using MediatR;
using Messaging.Application.Commands.DeleteMessage;
using Messaging.Application.Commands.EditMessage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("{messageId}")]
    public async Task<IActionResult> Edit(string messageId, [FromBody] EditMessageRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new EditMessageCommand(messageId, GetUserId(), request.Text), ct);
        return result.IsFailure
            ? BadRequest(ApiError.FromMessage(result.Error!, "MESSAGE_EDIT_FAILED"))
            : Ok(result.Value);
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> Delete(string messageId, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteMessageCommand(messageId, GetUserId()), ct);
        if (result.IsFailure)
            return result.Error!.Contains("не найдено", StringComparison.OrdinalIgnoreCase)
                ? NotFound(ApiError.FromMessage(result.Error, "MESSAGE_NOT_FOUND"))
                : BadRequest(ApiError.FromMessage(result.Error, "FORBIDDEN"));

        return Ok(new { message = "Сообщение удалено" });
    }

    private string GetUserId() =>
        User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
}

public class EditMessageRequest
{
    public string Text { get; set; } = string.Empty;
}
