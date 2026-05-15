// TagDto.cs

namespace Courses.Application.Tags;

// Компонент тегов class: обслуживает поиск, DTO или синхронизацию тегов курса.
public class TagDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}
