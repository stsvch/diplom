using EduPlatform.Shared.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.DTOs;
using Payments.Application.Interfaces;
using System.Security.Claims;

namespace EduPlatform.Host.Controllers;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = "Admin")]
public class AdminPaymentsController : ControllerBase
{
    private readonly IPaymentsService _paymentsService;

    public AdminPaymentsController(IPaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminPaymentRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _paymentsService.GetAdminPaymentRecordsAsync(search, page, pageSize, cancellationToken));
    }

    [HttpGet("subscription-plans")]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionPlans(CancellationToken cancellationToken)
    {
        return Ok(await _paymentsService.GetAdminSubscriptionPlansAsync(cancellationToken));
    }

    [HttpGet("subscription-allocation-runs")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminSubscriptionAllocationRunDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionAllocationRuns(
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _paymentsService.GetAdminSubscriptionAllocationRunsAsync(take, cancellationToken));
    }

    [HttpPost("subscription-plans")]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubscriptionPlan(
        [FromBody] UpsertSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _paymentsService.CreateSubscriptionPlanAsync(
                request.Name,
                request.Description,
                request.Price,
                request.Currency,
                request.BillingInterval,
                request.BillingIntervalCount,
                request.IsActive,
                request.IsFeatured,
                request.SortOrder,
                request.ProviderProductId,
                request.ProviderPriceId,
                cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "SUBSCRIPTION_PLAN_CREATE_FAILED"));
        }
    }

    [HttpPut("subscription-plans/{subscriptionPlanId:guid}")]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSubscriptionPlan(
        Guid subscriptionPlanId,
        [FromBody] UpsertSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _paymentsService.UpdateSubscriptionPlanAsync(
                subscriptionPlanId,
                request.Name,
                request.Description,
                request.Price,
                request.Currency,
                request.BillingInterval,
                request.BillingIntervalCount,
                request.IsActive,
                request.IsFeatured,
                request.SortOrder,
                request.ProviderProductId,
                request.ProviderPriceId,
                cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "SUBSCRIPTION_PLAN_UPDATE_FAILED"));
        }
    }

    [HttpPost("refunds")]
    [ProducesResponseType(typeof(RefundRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRefund(
        [FromBody] AdminRefundRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(adminId))
            return Unauthorized();

        try
        {
            var result = await _paymentsService.CreateAdminRefundAsync(
                request.PaymentAttemptId,
                request.Amount,
                request.Reason,
                adminId,
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "ADMIN_REFUND_FAILED"));
        }
    }
}

public record AdminRefundRequest(Guid PaymentAttemptId, decimal? Amount, string? Reason);
public record UpsertSubscriptionPlanRequest(
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    string BillingInterval,
    int BillingIntervalCount,
    bool IsActive,
    bool IsFeatured,
    int SortOrder,
    string? ProviderProductId,
    string? ProviderPriceId);
