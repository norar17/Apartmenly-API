using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Maintenance.DTOs;
using ApartmentRental.Application.Maintenance.Interfaces;
using ApartmentRental.Shared;
using ApartmentRental.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApartmentRental.API.Controllers;

[Authorize]
[Route("api/maintenance")]
public class MaintenanceController : ApiControllerBase
{
    private readonly IMaintenanceService _maintenanceService;

    public MaintenanceController(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.OwnerOnly)]
    public async Task<ActionResult<ApiResponse<PagedResult<MaintenanceRequestDto>>>> GetAll(
        [FromQuery] PaginationParams pagination, [FromQuery] string? status, [FromQuery] string? priority, CancellationToken cancellationToken)
    {
        var result = await _maintenanceService.GetRequestsAsync(pagination, status, priority, cancellationToken);
        return Ok(ApiResponse<PagedResult<MaintenanceRequestDto>>.Ok(result));
    }

    [HttpGet("mine")]
    [Authorize(Policy = Policies.RenterOnly)]
    public async Task<ActionResult<ApiResponse<PagedResult<MaintenanceRequestDto>>>> GetMine([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        var result = await _maintenanceService.GetRequestsForRenterAsync(CurrentUserId, pagination, cancellationToken);
        return Ok(ApiResponse<PagedResult<MaintenanceRequestDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = Policies.RenterOnly)]
    public async Task<ActionResult<ApiResponse<MaintenanceRequestDto>>> Create(CreateMaintenanceRequestRequest request, CancellationToken cancellationToken)
        => FromResult(await _maintenanceService.CreateRequestAsync(CurrentUserId, request, cancellationToken));

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = Policies.OwnerOnly)]
    public async Task<ActionResult<ApiResponse<MaintenanceRequestDto>>> UpdateStatus(Guid id, UpdateMaintenanceStatusRequest request, CancellationToken cancellationToken)
        => FromResult(await _maintenanceService.UpdateStatusAsync(id, request, cancellationToken));
}
