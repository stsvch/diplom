// ParticipantDto.cs
namespace Messaging.Application.DTOs;

// DTO задаёт форму данных, которую application-слой возвращает API и клиенту.
public class ParticipantDto
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
