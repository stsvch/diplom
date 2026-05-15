// ContentMappingProfile.cs

using System.Text.Json;
using AutoMapper;
using Content.Application.DTOs;
using Content.Domain.Entities;

namespace Content.Application.Mappings;

// AutoMapper-профиль class: группирует правила преобразования доменных моделей и DTO.
public class ContentMappingProfile : Profile
{
    // Здесь описаны преобразования между доменными сущностями и DTO application layer.
    public ContentMappingProfile()
    {
        CreateMap<Attachment, AttachmentDto>();

        CreateMap<LessonBlock, LessonBlockDto>()
            .ForMember(d => d.ValidationErrors, opt => opt.MapFrom(s => DeserializeErrors(s.ValidationErrorsJson)));

        CreateMap<LessonBlockAttempt, LessonBlockAttemptDto>();
    }

    private static IReadOnlyList<string> DeserializeErrors(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
