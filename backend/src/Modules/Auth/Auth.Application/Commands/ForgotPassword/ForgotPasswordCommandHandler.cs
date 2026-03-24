using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using EduPlatform.Shared.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Always return success to prevent email enumeration attacks
        if (user == null)
            return Result.Success<string>("If that email is registered, a password reset link has been sent.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendPasswordResetAsync(user.Email!, token, cancellationToken);

        return Result.Success<string>("If that email is registered, a password reset link has been sent.");
    }
}
