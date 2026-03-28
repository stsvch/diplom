using AutoMapper;
using Calendar.Application.DTOs;
using Calendar.Domain.Entities;

namespace Calendar.Application.Mappings;

public class CalendarMappingProfile : Profile
{
    public CalendarMappingProfile()
    {
        CreateMap<CalendarEvent, CalendarEventDto>();
    }
}
