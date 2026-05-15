// TeacherWithScheduleDto.cs

namespace Scheduling.Application.DTOs;

/// <summary>
/// DTO описывает данные TeacherWithScheduleDto, передаваемые наружу без прямой отдачи доменных сущностей.
/// </summary>
public record TeacherWithScheduleDto(
    string TeacherId,
    string TeacherName,
    int ActiveRulesCount,
    int IndividualRulesCount,
    int GroupRulesCount);
