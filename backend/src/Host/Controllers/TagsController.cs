// Файл: TagsController.cs
using Courses.Application.Tags;
using Courses.Application.Tags.Queries.SearchTags;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPlatform.Host.Controllers;

// Контроллер TagsController группирует HTTP-эндпоинты и делегирует работу в прикладные сценарии.
[ApiController]
[Route("api/tags")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Поиск тегов для автодополнения. Возвращает существующие теги по подстроке slug или name,
    /// отсортированные по популярности (UsageCount desc).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new SearchTagsQuery(q, limit), cancellationToken);
        return Ok(result);
    }
}
