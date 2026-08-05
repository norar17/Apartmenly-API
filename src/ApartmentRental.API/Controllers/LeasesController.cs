using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Leases.DTOs;
using ApartmentRental.Application.Leases.Interfaces;
using ApartmentRental.Shared;
using ApartmentRental.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApartmentRental.API.Controllers;

[Authorize(Policy = Policies.OwnerOnly)]
public class LeasesController : ApiControllerBase
{
    private readonly ILeaseService _leaseService;

    public LeasesController(ILeaseService leaseService)
    {
        _leaseService = leaseService;
    }

    [HttpGet("mine")]
    [Authorize(Policy = Policies.RenterOnly)]
    public async Task<ActionResult<ApiResponse<LeaseDto>>> GetMine(CancellationToken cancellationToken)
        => FromResult(await _leaseService.GetMyActiveLeaseAsync(CurrentUserId, cancellationToken));

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<LeaseDto>>>> GetAll([FromQuery] PaginationParams pagination, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var result = await _leaseService.GetLeasesAsync(pagination, status, cancellationToken);
        return Ok(ApiResponse<PagedResult<LeaseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<LeaseDto>>> GetById(Guid id, CancellationToken cancellationToken)
        => FromResult(await _leaseService.GetLeaseByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LeaseDto>>> Create(CreateLeaseRequest request, CancellationToken cancellationToken)
        => FromResult(await _leaseService.CreateLeaseAsync(request, cancellationToken));

    [HttpPost("{id:guid}/renew")]
    public async Task<ActionResult<ApiResponse<LeaseDto>>> Renew(Guid id, RenewLeaseRequest request, CancellationToken cancellationToken)
        => FromResult(await _leaseService.RenewLeaseAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/terminate")]
    public async Task<IActionResult> Terminate(Guid id, TerminateLeaseRequest request, CancellationToken cancellationToken)
        => FromResult(await _leaseService.TerminateLeaseAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/move-out")]
    public async Task<IActionResult> MoveOut(Guid id, CancellationToken cancellationToken)
        => FromResult(await _leaseService.MoveOutAsync(id, cancellationToken));
}
