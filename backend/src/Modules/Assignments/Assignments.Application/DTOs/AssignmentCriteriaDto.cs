// AssignmentCriteriaDto.cs

namespace Assignments.Application.DTOs;

/// <summary>
/// Критерий оценивания в ответах API: id, текст критерия, максимальные баллы и порядок отображения.
/// </summary>
public class AssignmentCriteriaDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int MaxPoints { get; set; }
    public int OrderIndex { get; set; }
}

/// Входная модель критерия из формы создания/редактирования: текст пункта и максимальный балл.
public record AssignmentCriteriaInput(string Text, int MaxPoints);
