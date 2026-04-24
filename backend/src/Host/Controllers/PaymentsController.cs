using EduPlatform.Shared.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.DTOs;
using Payments.Application.Interfaces;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentsService _paymentsService;

    public PaymentsController(IPaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

    [HttpPost("course-checkout")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(CourseCheckoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCourseCheckout(
        [FromBody] CreateCourseCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        try
        {
            var result = await _paymentsService.CreateCourseCheckoutAsync(
                request.CourseId,
                studentId,
                GetUserEmail(),
                GetUserFullName("Студент"),
                request.SavePaymentMethod,
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "CHECKOUT_FAILED"));
        }
    }

    [HttpPost("subscription-checkout")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(SubscriptionCheckoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubscriptionCheckout(
        [FromBody] CreateSubscriptionCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        try
        {
            var result = await _paymentsService.CreateSubscriptionCheckoutAsync(
                request.SubscriptionPlanId,
                studentId,
                GetUserEmail(),
                GetUserFullName("Студент"),
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "SUBSCRIPTION_CHECKOUT_FAILED"));
        }
    }

    [HttpGet("me/history")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentAttemptDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPaymentHistory(CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        return Ok(await _paymentsService.GetMyPaymentHistoryAsync(studentId, cancellationToken));
    }

    [HttpGet("me/subscriptions")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<UserSubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMySubscriptions(CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        return Ok(await _paymentsService.GetMySubscriptionsAsync(studentId, cancellationToken));
    }

    [HttpGet("me/subscription-history")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionPaymentAttemptDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMySubscriptionHistory(CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        return Ok(await _paymentsService.GetMySubscriptionHistoryAsync(studentId, cancellationToken));
    }

    [HttpGet("me/subscription-invoices")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionInvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMySubscriptionInvoices(CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        return Ok(await _paymentsService.GetMySubscriptionInvoicesAsync(studentId, cancellationToken));
    }

    [HttpGet("me/purchases")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<CoursePurchaseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPurchases(CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        return Ok(await _paymentsService.GetMyPurchasesAsync(studentId, cancellationToken));
    }

    [HttpGet("me/refunds")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<RefundRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyRefunds(CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        return Ok(await _paymentsService.GetMyRefundsAsync(studentId, cancellationToken));
    }

    [HttpGet("me/disputes")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<DisputeRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyDisputes(CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        return Ok(await _paymentsService.GetMyDisputesAsync(studentId, cancellationToken));
    }

    [HttpGet("me/payment-methods")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentMethodRefDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPaymentMethods(CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        return Ok(await _paymentsService.GetMyPaymentMethodsAsync(studentId, cancellationToken));
    }

    [HttpGet("subscription-plans")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionPlans(CancellationToken cancellationToken)
    {
        return Ok(await _paymentsService.GetActiveSubscriptionPlansAsync(cancellationToken));
    }

    [HttpDelete("me/payment-methods/{paymentMethodId:guid}")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveMyPaymentMethod(Guid paymentMethodId, CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        try
        {
            await _paymentsService.RemoveMyPaymentMethodAsync(paymentMethodId, studentId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "PAYMENT_METHOD_REMOVE_FAILED"));
        }
    }

    [HttpGet("attempts/{paymentAttemptId:guid}")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(PaymentAttemptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentAttempt(Guid paymentAttemptId, CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        var result = await _paymentsService.GetPaymentAttemptAsync(paymentAttemptId, studentId, cancellationToken);
        if (result == null)
            return NotFound(ApiError.FromMessage("Попытка оплаты не найдена.", "PAYMENT_ATTEMPT_NOT_FOUND"));

        return Ok(result);
    }

    [HttpPost("attempts/{paymentAttemptId:guid}/cancel-return")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(PaymentAttemptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkPaymentAttemptCanceled(Guid paymentAttemptId, CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        var result = await _paymentsService.MarkPaymentAttemptCanceledAsync(paymentAttemptId, studentId, cancellationToken);
        if (result == null)
            return NotFound(ApiError.FromMessage("Попытка оплаты не найдена.", "PAYMENT_ATTEMPT_NOT_FOUND"));

        return Ok(result);
    }

    [HttpGet("subscription-attempts/{subscriptionPaymentAttemptId:guid}")]
    [Authorize(Roles = "Student")]
    [ProducesResponseType(typeof(SubscriptionPaymentAttemptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscriptionPaymentAttempt(
        Guid subscriptionPaymentAttemptId,
        CancellationToken cancellationToken)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        var result = await _paymentsService.GetSubscriptionPaymentAttemptAsync(
            subscriptionPaymentAttemptId,
            studentId,
            cancellationToken);

        if (result == null)
            return NotFound(ApiError.FromMessage("Попытка оформления подписки не найдена.", "SUBSCRIPTION_ATTEMPT_NOT_FOUND"));

        return Ok(result);
    }

    [HttpGet("teacher/payout-account")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(TeacherPayoutAccountDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeacherPayoutAccount(CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        return Ok(await _paymentsService.GetTeacherPayoutAccountAsync(teacherId, cancellationToken));
    }

    [HttpPost("teacher/payout-account/onboarding-link")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTeacherOnboardingLink(CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        try
        {
            var url = await _paymentsService.CreateTeacherOnboardingLinkAsync(
                teacherId,
                GetUserEmail(),
                GetUserFullName("Преподаватель"),
                cancellationToken);

            return Ok(new { url });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "PAYOUT_ONBOARDING_FAILED"));
        }
    }

    [HttpPost("teacher/payout-account/dashboard-link")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTeacherDashboardLink(CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        try
        {
            var url = await _paymentsService.CreateTeacherDashboardLinkAsync(
                teacherId,
                cancellationToken);

            return Ok(new { url });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "PAYOUT_DASHBOARD_LINK_FAILED"));
        }
    }

    [HttpGet("teacher/summary")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(TeacherSettlementSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeacherSettlementSummary(CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        return Ok(await _paymentsService.GetTeacherSettlementSummaryAsync(teacherId, cancellationToken));
    }

    [HttpGet("teacher/settlements")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(IReadOnlyList<TeacherSettlementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeacherSettlements(CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        return Ok(await _paymentsService.GetTeacherSettlementsAsync(teacherId, cancellationToken));
    }

    [HttpGet("teacher/subscription-allocations")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(IReadOnlyList<TeacherSubscriptionAllocationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeacherSubscriptionAllocations(CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        return Ok(await _paymentsService.GetTeacherSubscriptionAllocationsAsync(teacherId, cancellationToken));
    }

    [HttpGet("teacher/payouts")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(IReadOnlyList<PayoutRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeacherPayouts(CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        return Ok(await _paymentsService.GetTeacherPayoutRecordsAsync(teacherId, cancellationToken));
    }

    [HttpGet("teacher/disputes")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(IReadOnlyList<DisputeRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeacherDisputes(CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        return Ok(await _paymentsService.GetTeacherDisputesAsync(teacherId, cancellationToken));
    }

    [HttpPost("teacher/payouts/request")]
    [Authorize(Roles = "Teacher")]
    [ProducesResponseType(typeof(PayoutRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestTeacherPayout(CancellationToken cancellationToken)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        try
        {
            return Ok(await _paymentsService.RequestTeacherPayoutAsync(teacherId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "PAYOUT_REQUEST_FAILED"));
        }
    }

    [HttpPost("webhooks/stripe")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleStripeWebhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signatureHeader = Request.Headers["Stripe-Signature"].FirstOrDefault();

        try
        {
            await _paymentsService.HandleStripeWebhookAsync(payload, signatureHeader, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "STRIPE_WEBHOOK_FAILED"));
        }
    }

    private string GetUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? $"{User.FindFirstValue(ClaimTypes.NameIdentifier)}@placeholder.local";

    private string GetUserFullName(string fallback)
    {
        var given = User.FindFirstValue(ClaimTypes.GivenName) ?? "";
        var surname = User.FindFirstValue(ClaimTypes.Surname) ?? "";
        var fullName = $"{given} {surname}".Trim();
        if (!string.IsNullOrWhiteSpace(fullName))
            return fullName;

        return User.FindFirstValue(ClaimTypes.Name) ?? fallback;
    }
}

public record CreateCourseCheckoutRequest(Guid CourseId, bool SavePaymentMethod = false);
public record CreateSubscriptionCheckoutRequest(Guid SubscriptionPlanId);
