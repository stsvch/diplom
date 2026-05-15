// UpdateCourseCommandHandler.cs

using Ardalis.Specification.EntityFrameworkCore;
using AutoMapper;
using Courses.Application.DTOs;
using Courses.Application.Interfaces;
using Courses.Application.Specifications;
using EduPlatform.Shared.Application.Contracts;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Courses.Application.Courses.Commands.UpdateCourse;

// Тип class: ключевой элемент файла UpdateCourseCommandHandler.cs.
public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, Result<CourseDetailDto>>
{
    private readonly ICoursesDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITeacherPayoutReadService _teacherPayoutReadService;
    private readonly IChatAdmin _chatAdmin;

    public UpdateCourseCommandHandler(
        ICoursesDbContext context,
        IMapper mapper,
        ITeacherPayoutReadService teacherPayoutReadService,
        IChatAdmin chatAdmin)
    {
        _context = context;
        _mapper = mapper;
        _teacherPayoutReadService = teacherPayoutReadService;
        _chatAdmin = chatAdmin;
    }

    // Основной сценарий handler-а: загружает нужные данные, применяет правила и формирует ответ.
    public async Task<Result<CourseDetailDto>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var spec = new CourseByIdSpec(request.Id);
        var course = await _context.Courses
            .WithSpecification(spec)
            .FirstOrDefaultAsync(cancellationToken);

        if (course == null)
            return Result.Failure<CourseDetailDto>("Курс не найден.");

        if (course.TeacherId != request.TeacherId)
            return Result.Failure<CourseDetailDto>("Вы не можете редактировать чужой курс.");

        if (course.IsArchived)
            return Result.Failure<CourseDetailDto>("Архивированный курс нельзя редактировать.");

        if (!request.IsFree && (!request.Price.HasValue || request.Price.Value <= 0))
            return Result.Failure<CourseDetailDto>("Для платного курса цена должна быть больше 0.");

        if (course.IsPublished && course.IsFree && !request.IsFree)
        {
            var payoutReady = await _teacherPayoutReadService.IsTeacherReadyForPaidCoursesAsync(
                request.TeacherId,
                cancellationToken);

            if (!payoutReady)
                return Result.Failure<CourseDetailDto>("Нельзя перевести опубликованный курс в платный без подключённых выплат.");
        }

        course.DisciplineId = request.DisciplineId;
        course.Title = request.Title;
        course.Description = request.Description;
        course.Price = request.IsFree ? null : request.Price;
        course.IsFree = request.IsFree;
        course.OrderType = request.OrderType;
        course.Level = request.Level;
        course.ImageUrl = request.ImageUrl;
        course.Deadline = request.Deadline;

        await _context.SaveChangesAsync(cancellationToken);
        await _chatAdmin.UpdateCourseChatNameAsync(
            course.Id.ToString(),
            course.Title,
            cancellationToken);

        return Result.Success(_mapper.Map<CourseDetailDto>(course));
    }
}
