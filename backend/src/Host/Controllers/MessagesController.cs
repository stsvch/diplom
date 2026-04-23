using EduPlatform.Shared.Application.Models;
using Messaging.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private static readonly TimeSpan EditWindow = TimeSpan.FromMinutes(15);

    private readonly IMessagingRepository _repository;
    private readonly IChatBroadcaster _broadcaster;

    public MessagesController(IMessagingRepository repository, IChatBroadcaster broadcaster)
    {
        _repository = repository;
        _broadcaster = broadcaster;
    }

    [HttpPut("{messageId}")]
    public async Task<IActionResult> Edit(string messageId, [FromBody] EditMessageRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(ApiError.FromMessage("Пустой текст", "EMPTY_MESSAGE"));

        var edited = await _repository.EditMessageAsync(messageId, userId, request.Text, EditWindow);
        if (!edited)
            return BadRequest(ApiError.FromMessage(
                "Сообщение не найдено, не ваше или истёк срок редактирования (15 мин).",
                "MESSAGE_EDIT_FAILED"));

        var updated = await _repository.GetMessageByIdAsync(messageId);
        if (updated == null)
            return NotFound();

        var dto = ChatsController.MapMessageToDto(updated);
        await _broadcaster.MessageEditedAsync(updated.ChatId, dto);
        return Ok(dto);
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> Delete(string messageId)
    {
        var userId = GetUserId();
        var message = await _repository.GetMessageByIdAsync(messageId);
        if (message == null)
            return NotFound(ApiError.FromMessage("Сообщение не найдено", "MESSAGE_NOT_FOUND"));

        var deleted = await _repository.DeleteMessageAsync(messageId, userId);
        if (!deleted)
            return BadRequest(ApiError.FromMessage("Нельзя удалить чужое сообщение", "FORBIDDEN"));

        await _broadcaster.MessageDeletedAsync(message.ChatId, messageId);
        return Ok(new { message = "Сообщение удалено" });
    }

    private string GetUserId() =>
        User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
}

public class EditMessageRequest
{
    public string Text { get; set; } = string.Empty;
}
