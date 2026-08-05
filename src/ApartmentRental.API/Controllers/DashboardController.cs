using ApartmentRental.Application.Dashboard.DTOs;
using ApartmentRental.Application.Dashboard.Interfaces;
using ApartmentRental.Shared;
using ApartmentRental.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApartmentRental.API.Controllers;

[Authorize(Policy = Policies.OwnerOnly)]
public class DashboardController : ApiControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<OwnerDashboardDto>>> Get(CancellationToken cancellationToken)
    {
        var dashboard = await _dashboardService.GetOwnerDashboardAsync(CurrentUserId, cancellationToken);
        return Ok(ApiResponse<OwnerDashboardDto>.Ok(dashboard));
    }
}
