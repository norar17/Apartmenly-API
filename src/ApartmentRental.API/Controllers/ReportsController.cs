using ApartmentRental.Application.Reports.DTOs;
using ApartmentRental.Application.Reports.Interfaces;
using ApartmentRental.Shared;
using ApartmentRental.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApartmentRental.API.Controllers;

[Authorize(Policy = Policies.OwnerOnly)]
public class ReportsController : ApiControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("rent-collection")]
    public async Task<ActionResult<ApiResponse<List<RentCollectionReportRowDto>>>> RentCollection([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var rows = await _reportService.GetRentCollectionReportAsync(new ReportDateRangeRequest(from, to), cancellationToken);
        return Ok(ApiResponse<List<RentCollectionReportRowDto>>.Ok(rows));
    }

    [HttpGet("occupancy")]
    public async Task<ActionResult<ApiResponse<List<OccupancyReportRowDto>>>> Occupancy(CancellationToken cancellationToken)
    {
        var rows = await _reportService.GetOccupancyReportAsync(cancellationToken);
        return Ok(ApiResponse<List<OccupancyReportRowDto>>.Ok(rows));
    }

    [HttpGet("late-payments")]
    public async Task<ActionResult<ApiResponse<List<LatePaymentReportRowDto>>>> LatePayments(CancellationToken cancellationToken)
    {
        var rows = await _reportService.GetLatePaymentReportAsync(cancellationToken);
        return Ok(ApiResponse<List<LatePaymentReportRowDto>>.Ok(rows));
    }

    [HttpGet("maintenance")]
    public async Task<ActionResult<ApiResponse<List<MaintenanceReportRowDto>>>> Maintenance([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var rows = await _reportService.GetMaintenanceReportAsync(new ReportDateRangeRequest(from, to), cancellationToken);
        return Ok(ApiResponse<List<MaintenanceReportRowDto>>.Ok(rows));
    }
}
