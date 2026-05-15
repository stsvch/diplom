// CalendarMappingProfile.cs
using AutoMapper;
using Calendar.Application.DTOs;
using Calendar.Domain.Entities;

namespace Calendar.Application.Mappings;

// Маппинг централизует преобразование доменных или Mongo-моделей в DTO.
public class CalendarMappingProfile : Profile
{
    public CalendarMappingProfile()
    {
        CreateMap<CalendarEvent, CalendarEventDto>();
    }
}
