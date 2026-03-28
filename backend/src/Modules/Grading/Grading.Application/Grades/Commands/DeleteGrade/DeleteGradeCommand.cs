using EduPlatform.Shared.Domain;
using MediatR;

namespace Grading.Application.Grades.Commands.DeleteGrade;

public record DeleteGradeCommand(Guid Id, string RequesterId) : IRequest<Result>;
