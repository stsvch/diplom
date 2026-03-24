using Courses.Application.DTOs;
using EduPlatform.Shared.Domain;
using MediatR;

namespace Courses.Application.Modules.Commands.CreateModule;

public record CreateModuleCommand(
    Guid CourseId,
    string Title,
    string? Description
) : IRequest<Result<CourseModuleDto>>;
