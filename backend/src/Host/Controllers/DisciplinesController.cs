using Courses.Application.Disciplines.Commands.CreateDiscipline;
using Courses.Application.Disciplines.Commands.DeleteDiscipline;
using Courses.Application.Disciplines.Commands.UpdateDiscipline;
using Courses.Application.Disciplines.Queries.GetDisciplineById;
using Courses.Application.Disciplines.Queries.GetDisciplines;
using Courses.Application.DTOs;
using EduPlatform.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/disciplines")]
public class DisciplinesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DisciplinesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DisciplineDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDisciplinesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DisciplineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDisciplineByIdQuery(id), cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "DISCIPLINE_NOT_FOUND"));

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DisciplineDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDisciplineCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return BadRequest(ApiError.FromMessage(result.Error!, "DISCIPLINE_CREATE_FAILED"));

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DisciplineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDisciplineRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateDisciplineCommand(id, request.Name, request.Description, request.ImageUrl);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "DISCIPLINE_NOT_FOUND"));

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteDisciplineCommand(id), cancellationToken);
        if (result.IsFailure)
            return NotFound(ApiError.FromMessage(result.Error!, "DISCIPLINE_NOT_FOUND"));

        return Ok(new { message = result.Value });
    }
}

public record UpdateDisciplineRequest(string Name, string? Description, string? ImageUrl);
