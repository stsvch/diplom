// UpdateModuleCommand.cs

using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Modules.Commands.UpdateModule;

// Тип record: ключевой элемент файла UpdateModuleCommand.cs.
public record UpdateModuleCommand(
    Guid Id,
    string Title,
    string? Description,
    bool? IsPublished
) : IRequest<Result<CourseModuleDto>>;
