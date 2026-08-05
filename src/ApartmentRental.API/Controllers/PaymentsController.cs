using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Payments.DTOs;
using ApartmentRental.Application.Payments.Interfaces;
using ApartmentRental.Shared;
using ApartmentRental.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApartmentRental.API.Controllers;

[Authorize]
public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.OwnerOnly)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentDto>>>> GetAll(
        [FromQuery] PaginationParams pagination, [FromQuery] string? status,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetPaymentsAsync(pagination, status, from, to, cancellationToken);
        return Ok(ApiResponse<PagedResult<PaymentDto>>.Ok(result));
    }

    [HttpGet("mine")]
    [Authorize(Policy = Policies.RenterOnly)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentDto>>>> GetMine([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetPaymentsForRenterAsync(CurrentUserId, pagination, cancellationToken);
        return Ok(ApiResponse<PagedResult<PaymentDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = Policies.OwnerOnly)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> Create(CreatePaymentRequest request, CancellationToken cancellationToken)
        => FromResult(await _paymentService.CreatePaymentAsync(request, cancellationToken));

    [HttpPost("{id:guid}/mark-paid")]
    [Authorize(Policy = Policies.OwnerOnly)]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> MarkAsPaid(Guid id, MarkPaymentPaidRequest request, CancellationToken cancellationToken)
        => FromResult(await _paymentService.MarkAsPaidAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = Policies.OwnerOnly)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        => FromResult(await _paymentService.CancelPaymentAsync(id, cancellationToken));
}
