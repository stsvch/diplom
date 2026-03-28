using AutoMapper;
using Notifications.Application.DTOs;
using Notifications.Domain.Entities;

namespace Notifications.Application.Mappings;

public class NotificationsMappingProfile : Profile
{
    public NotificationsMappingProfile()
    {
        CreateMap<Notification, NotificationDto>();
    }
}
