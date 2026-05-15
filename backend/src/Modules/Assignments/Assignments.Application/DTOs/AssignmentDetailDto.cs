// AssignmentDetailDto.cs

namespace Assignments.Application.DTOs;

/// <summary>
/// Детальная карточка задания расширяет AssignmentDto списком сдач, чтобы преподаватель видел работы внутри одного запроса.
/// </summary>
public class AssignmentDetailDto : AssignmentDto
{
    public List<SubmissionDto> Submissions { get; set; } = new();
}
