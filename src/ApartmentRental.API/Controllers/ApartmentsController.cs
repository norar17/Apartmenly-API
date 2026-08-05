using ApartmentRental.Application.Apartments.DTOs;
using ApartmentRental.Application.Apartments.Interfaces;
using ApartmentRental.Application.Common.Models;
using ApartmentRental.Shared;
using ApartmentRental.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApartmentRental.API.Controllers;

[Authorize(Policy = Policies.OwnerOnly)]
public class ApartmentsController : ApiControllerBase
{
    private readonly IApartmentService _apartmentService;

    public ApartmentsController(IApartmentService apartmentService)
    {
        _apartmentService = apartmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ApartmentDto>>>> GetAll([FromQuery] PaginationParams pagination, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var result = await _apartmentService.GetApartmentsAsync(CurrentUserId, pagination, status, cancellationToken);
        return Ok(ApiResponse<PagedResult<ApartmentDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ApartmentDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
        => FromResult(await _apartmentService.GetApartmentByIdAsync(CurrentUserId, id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ApartmentDto>>> Create(CreateApartmentRequest request, CancellationToken cancellationToken)
        => FromResult(await _apartmentService.CreateApartmentAsync(CurrentUserId, request, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ApartmentDto>>> Update(Guid id, UpdateApartmentRequest request, CancellationToken cancellationToken)
        => FromResult(await _apartmentService.UpdateApartmentAsync(CurrentUserId, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => FromResult(await _apartmentService.DeleteApartmentAsync(CurrentUserId, id, cancellationToken));
}
