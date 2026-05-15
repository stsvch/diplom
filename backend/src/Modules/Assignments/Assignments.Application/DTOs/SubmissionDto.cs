// SubmissionDto.cs

using Assignments.Domain.Enums;

namespace Assignments.Application.DTOs;

/// <summary>
/// Карточка сдачи студента: id задания, студент, номер попытки, текст ответа, дата отправки, статус, балл, максимум, комментарий и дата проверки.
/// </summary>
public class SubmissionDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public string? Content { get; set; }
    public DateTime SubmittedAt { get; set; }
    public SubmissionStatus Status { get; set; }
    public int? Score { get; set; }
    public int MaxScore { get; set; }
    public string? TeacherComment { get; set; }
    public DateTime? GradedAt { get; set; }
}
