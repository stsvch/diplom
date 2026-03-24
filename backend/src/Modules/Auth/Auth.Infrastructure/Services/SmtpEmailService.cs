using Auth.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Auth.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string userId, string token, CancellationToken cancellationToken = default)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
        var encodedToken = Uri.EscapeDataString(token);
        var confirmationLink = $"{frontendUrl}/confirm-email?userId={userId}&token={encodedToken}";

        var subject = "Confirm your email address";
        var body = $@"
            <h2>Email Confirmation</h2>
            <p>Thank you for registering. Please confirm your email address by clicking the link below:</p>
            <p><a href=""{confirmationLink}"">Confirm Email</a></p>
            <p>If you did not create an account, please ignore this email.</p>
            <p>This link will expire in 24 hours.</p>";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string toEmail, string token, CancellationToken cancellationToken = default)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(toEmail);
        var resetLink = $"{frontendUrl}/reset-password?email={encodedEmail}&token={encodedToken}";

        var subject = "Reset your password";
        var body = $@"
            <h2>Password Reset</h2>
            <p>You have requested to reset your password. Click the link below to proceed:</p>
            <p><a href=""{resetLink}"">Reset Password</a></p>
            <p>If you did not request a password reset, please ignore this email.</p>
            <p>This link will expire in 1 hour.</p>";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var host = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
        var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var fromEmail = _configuration["Smtp:FromEmail"];
        var fromName = _configuration["Smtp:FromName"] ?? "EduPlatform";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(fromEmail) || username == "your-email@gmail.com")
        {
            _logger.LogWarning(
                "SMTP is not configured. Email to {ToEmail} with subject '{Subject}' was NOT sent. " +
                "Configure Smtp:Username, Smtp:Password, Smtp:FromEmail in appsettings or environment variables.",
                toEmail, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(string.Empty, toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(username, password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
