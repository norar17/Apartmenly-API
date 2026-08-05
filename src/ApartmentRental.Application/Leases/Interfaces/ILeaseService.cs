using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Leases.DTOs;
using ApartmentRental.Shared;

namespace ApartmentRental.Application.Leases.Interfaces;

public interface ILeaseService
{
    Task<PagedResult<LeaseDto>> GetLeasesAsync(PaginationParams pagination, string? status, CancellationToken cancellationToken = default);
    Task<Result<LeaseDto>> GetLeaseByIdAsync(Guid leaseId, CancellationToken cancellationToken = default);
    Task<Result<LeaseDto>> CreateLeaseAsync(CreateLeaseRequest request, CancellationToken cancellationToken = default);
    Task<Result<LeaseDto>> RenewLeaseAsync(Guid leaseId, RenewLeaseRequest request, CancellationToken cancellationToken = default);
    Task<Result> TerminateLeaseAsync(Guid leaseId, TerminateLeaseRequest request, CancellationToken cancellationToken = default);
    Task<Result> MoveOutAsync(Guid leaseId, CancellationToken cancellationToken = default);
    Task<Result<LeaseDto>> GetMyActiveLeaseAsync(Guid renterId, CancellationToken cancellationToken = default);
}
