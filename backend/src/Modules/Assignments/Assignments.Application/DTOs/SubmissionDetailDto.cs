// SubmissionDetailDto.cs

namespace Assignments.Application.DTOs;

/// <summary>
/// Детальная сдача дополняет SubmissionDto именами файлов-вложений, которые показываются преподавателю при проверке.
/// </summary>
public class SubmissionDetailDto : SubmissionDto
{
    public List<string> AttachmentFileNames { get; set; } = new();
}
