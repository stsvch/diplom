// CreateModuleCommand.cs

using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Modules.Commands.CreateModule;

// Тип record: ключевой элемент файла CreateModuleCommand.cs.
public record CreateModuleCommand(
    Guid CourseId,
    string Title,
    string? Description
) : IRequest<Result<CourseModuleDto>>;
