using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Renters.DTOs;
using ApartmentRental.Application.Renters.Interfaces;
using ApartmentRental.Shared;
using ApartmentRental.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApartmentRental.API.Controllers;

[Authorize(Policy = Policies.OwnerOnly)]
public class RentersController : ApiControllerBase
{
    private readonly IRenterService _renterService;

    public RentersController(IRenterService renterService)
    {
        _renterService = renterService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<RenterDto>>>> GetAll([FromQuery] PaginationParams pagination, CancellationToken cancellationToken)
    {
        var result = await _renterService.GetRentersAsync(pagination, cancellationToken);
        return Ok(ApiResponse<PagedResult<RenterDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RenterDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
        => FromResult(await _renterService.GetRenterByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RenterDto>>> Create(CreateRenterRequest request, CancellationToken cancellationToken)
        => FromResult(await _renterService.CreateRenterAsync(request, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RenterDto>>> Update(Guid id, UpdateRenterRequest request, CancellationToken cancellationToken)
        => FromResult(await _renterService.UpdateRenterAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => FromResult(await _renterService.DeleteRenterAsync(id, cancellationToken));
}
