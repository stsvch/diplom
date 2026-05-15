// Файл: AdminPaymentsController.cs
using EduPlatform.Shared.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.DTOs;
using Payments.Application.Interfaces;

namespace EduPlatform.Host.Controllers;

// Контроллер AdminPaymentsController группирует HTTP-эндпоинты и делегирует работу в прикладные сценарии.
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
                request.IndividualSlotsPerMonth,
                request.GroupSlotsPerMonth,
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
                request.IndividualSlotsPerMonth,
                request.GroupSlotsPerMonth,
                cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiError.FromMessage(ex.Message, "SUBSCRIPTION_PLAN_UPDATE_FAILED"));
        }
    }

}

// API-модель UpsertSubscriptionPlanRequest фиксирует тело запроса или результат для действия контроллера.
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
    string? ProviderPriceId,
    int IndividualSlotsPerMonth,
    int GroupSlotsPerMonth);
