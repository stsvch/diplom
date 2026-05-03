using System.Text.Json;
using AutoMapper;
using Content.Application.DTOs;
using Content.Domain.Entities;

namespace Content.Application.Mappings;

public class ContentMappingProfile : Profile
{
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
