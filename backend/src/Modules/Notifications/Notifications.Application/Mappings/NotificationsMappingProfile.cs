// Файл: NotificationsMappingProfile.cs
using AutoMapper;
using Notifications.Application.DTOs;
using Notifications.Domain.Entities;

namespace Notifications.Application.Mappings;

// Профиль NotificationsMappingProfile содержит настройки AutoMapper для DTO и моделей модуля.
public class NotificationsMappingProfile : Profile
{
    public NotificationsMappingProfile()
    {
        CreateMap<Notification, NotificationDto>();
    }
}
