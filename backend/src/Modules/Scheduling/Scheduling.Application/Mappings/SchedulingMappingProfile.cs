// SchedulingMappingProfile.cs

using AutoMapper;
using Scheduling.Application.DTOs;
using Scheduling.Domain.Entities;

namespace Scheduling.Application.Mappings;

/// <summary>
/// Профиль AutoMapper собирает правила преобразования доменных сущностей в DTO модуля.
/// </summary>
public class SchedulingMappingProfile : Profile
{
    public SchedulingMappingProfile()
    {
        // Правила маппинга отделяют DTO application-слоя от EF/domain-моделей.
        CreateMap<SessionBooking, BookingDto>();

        CreateMap<ScheduleSlot, ScheduleSlotDto>()
            .ForMember(d => d.BookedCount,
                opt => opt.MapFrom(s => s.Bookings.Count(b => b.Status == Domain.Enums.BookingStatus.Booked)))
            .ForMember(d => d.Bookings,
                opt => opt.MapFrom(s => s.Bookings.ToList()));

        CreateMap<TeacherAvailability, TeacherAvailabilityDto>();
    }
}
