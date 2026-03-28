using AutoMapper;
using Scheduling.Application.DTOs;
using Scheduling.Domain.Entities;

namespace Scheduling.Application.Mappings;

public class SchedulingMappingProfile : Profile
{
    public SchedulingMappingProfile()
    {
        CreateMap<SessionBooking, BookingDto>();

        CreateMap<ScheduleSlot, ScheduleSlotDto>()
            .ForMember(d => d.BookedCount,
                opt => opt.MapFrom(s => s.Bookings.Count(b => b.Status == Domain.Enums.BookingStatus.Booked)))
            .ForMember(d => d.Bookings,
                opt => opt.MapFrom(s => s.Bookings.ToList()));
    }
}
