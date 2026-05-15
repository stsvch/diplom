// IEmailService.cs
namespace Auth.Application.Interfaces;

// Контракт отправки auth-писем: подтверждение email и восстановление пароля реализуются инфраструктурным SMTP-сервисом.
public interface IEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string userId, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string toEmail, string token, CancellationToken cancellationToken = default);
}
