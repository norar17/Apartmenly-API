using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Renters.DTOs;
using ApartmentRental.Shared;

namespace ApartmentRental.Application.Renters.Interfaces;

public interface IRenterService
{
    Task<PagedResult<RenterDto>> GetRentersAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<Result<RenterDetailDto>> GetRenterByIdAsync(Guid renterId, CancellationToken cancellationToken = default);
    Task<Result<RenterDto>> CreateRenterAsync(CreateRenterRequest request, CancellationToken cancellationToken = default);
    Task<Result<RenterDto>> UpdateRenterAsync(Guid renterId, UpdateRenterRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteRenterAsync(Guid renterId, CancellationToken cancellationToken = default);
}
