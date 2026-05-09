namespace Scheduling.Application.DTOs;

public record TeacherWithScheduleDto(
    string TeacherId,
    string TeacherName,
    int ActiveRulesCount,
    int IndividualRulesCount,
    int GroupRulesCount);
