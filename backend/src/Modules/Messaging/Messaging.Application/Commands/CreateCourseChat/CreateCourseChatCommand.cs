// CreateCourseChatCommand.cs
using EduPlatform.Shared.Domain;
using FluentValidation;
using MediatR;
using Messaging.Application.DTOs;
using Messaging.Application.Interfaces;
using Messaging.Application.Mappings;

namespace Messaging.Application.Commands.CreateCourseChat;

// Command содержит входные данные операции, изменяющей состояние модуля.
public record CreateCourseChatCommand(
    string OwnerId,
    string CourseId,
    string CourseName,
    IReadOnlyList<string> ParticipantIds,
    IReadOnlyList<string>? ParticipantNames
) : IRequest<Result<ChatDto>>;

// Валидатор проверяет параметры до выполнения handler-а.
public class CreateCourseChatValidator : AbstractValidator<CreateCourseChatCommand>
{
    public CreateCourseChatValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.CourseName).NotEmpty();
    }
}

// Handler выполняет сценарий через контекст или репозитории и возвращает Result/DTO.
public class CreateCourseChatCommandHandler : IRequestHandler<CreateCourseChatCommand, Result<ChatDto>>
{
    private readonly IMessagingRepository _repository;

    public CreateCourseChatCommandHandler(IMessagingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ChatDto>> Handle(CreateCourseChatCommand request, CancellationToken cancellationToken)
    {
        var chat = await _repository.CreateCourseChatAsync(
            request.CourseId,
            request.CourseName,
            request.ParticipantIds.ToList(),
            request.ParticipantNames?.ToList() ?? new List<string>(),
            ownerId: request.OwnerId);

        return Result.Success(chat.ToChatDto());
    }
}
