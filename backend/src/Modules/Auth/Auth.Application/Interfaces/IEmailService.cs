namespace Auth.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string userId, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string toEmail, string token, CancellationToken cancellationToken = default);
}
