using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Modules.Commands.UpdateModule;

public record UpdateModuleCommand(
    Guid Id,
    string Title,
    string? Description,
    bool? IsPublished
) : IRequest<Result<CourseModuleDto>>;
