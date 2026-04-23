using AutoMapper;
using Content.Application.DTOs;
using Content.Domain.Entities;

namespace Content.Application.Mappings;

public class ContentMappingProfile : Profile
{
    public ContentMappingProfile()
    {
        CreateMap<Attachment, AttachmentDto>();
        CreateMap<LessonBlock, LessonBlockDto>();
        CreateMap<LessonBlockAttempt, LessonBlockAttemptDto>();
    }
}
