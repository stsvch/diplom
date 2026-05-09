using AutoMapper;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.DTOs;
using Scheduling.Application.Interfaces;
using Scheduling.Application.Scheduling.Helpers;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Scheduling.Commands.CreateAvailability;

public class CreateAvailabilityCommandHandler : IRequestHandler<CreateAvailabilityCommand, Result<TeacherAvailabilityDto>>
{
    private readonly ISchedulingDbContext _context;
    private readonly IMapper _mapper;

    public CreateAvailabilityCommandHandler(ISchedulingDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TeacherAvailabilityDto>> Handle(CreateAvailabilityCommand request, CancellationToken cancellationToken)
    {
        if (request.SlotDurationMinutes <= 0)
            return Result.Failure<TeacherAvailabilityDto>("Длительность занятия должна быть больше нуля.");
        if (request.BreakBetweenMinutes < 0)
            return Result.Failure<TeacherAvailabilityDto>("Длительность перерыва не может быть отрицательной.");
        if (request.MaxStudents <= 0)
            return Result.Failure<TeacherAvailabilityDto>("Количество студентов должно быть больше нуля.");
        if (request.SessionType == SessionType.Individual && request.MaxStudents != 1)
            return Result.Failure<TeacherAvailabilityDto>("В индивидуальном занятии может быть только один студент.");
        if (request.EndTime <= request.StartTime)
            return Result.Failure<TeacherAvailabilityDto>("Конец интервала должен быть позже начала.");
        if (request.ValidUntil.HasValue && request.ValidUntil.Value < request.ValidFrom)
            return Result.Failure<TeacherAvailabilityDto>("Дата окончания не может быть раньше даты начала.");
        if (request.Kind == AvailabilityKind.Recurring && !request.DayOfWeek.HasValue)
            return Result.Failure<TeacherAvailabilityDto>("Для повторяющегося правила нужен день недели.");
        if (request.Kind == AvailabilityKind.OneOff && !request.SpecificDate.HasValue)
            return Result.Failure<TeacherAvailabilityDto>("Для разового правила нужна конкретная дата.");

        var existingRules = await _context.TeacherAvailabilities
            .Where(a => a.TeacherId == request.TeacherId)
            .ToListAsync(cancellationToken);

        var conflict = AvailabilityOverlapChecker.FindOverlap(
            existingRules,
            request.Kind,
            request.DayOfWeek,
            request.SpecificDate,
            request.StartTime,
            request.EndTime,
            request.ValidFrom,
            request.ValidUntil);

        if (conflict != null)
        {
            var when = conflict.Kind == AvailabilityKind.Recurring
                ? $"{conflict.DayOfWeek}, {conflict.StartTime:HH\\:mm}–{conflict.EndTime:HH\\:mm}"
                : $"{conflict.SpecificDate:dd.MM.yyyy}, {conflict.StartTime:HH\\:mm}–{conflict.EndTime:HH\\:mm}";
            return Result.Failure<TeacherAvailabilityDto>(
                $"Это время пересекается с существующим правилом «{conflict.Title}» ({when}).");
        }

        var availability = new TeacherAvailability
        {
            TeacherId = request.TeacherId,
            TeacherName = request.TeacherName,
            Kind = request.Kind,
            DayOfWeek = request.DayOfWeek,
            SpecificDate = request.SpecificDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SlotDurationMinutes = request.SlotDurationMinutes,
            BreakBetweenMinutes = request.BreakBetweenMinutes,
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
            SessionType = request.SessionType,
            MaxStudents = request.MaxStudents,
            Title = request.Title,
            Description = request.Description,
            MeetingLink = request.MeetingLink,
            RequiredCourseId = request.RequiredCourseId,
            IsActive = true
        };

        _context.TeacherAvailabilities.Add(availability);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<TeacherAvailabilityDto>(availability));
    }
}
