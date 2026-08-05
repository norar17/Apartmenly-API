using ApartmentRental.Application.Apartments.DTOs;
using ApartmentRental.Application.Common.Models;
using ApartmentRental.Shared;

namespace ApartmentRental.Application.Apartments.Interfaces;

public interface IApartmentService
{
    Task<PagedResult<ApartmentDto>> GetApartmentsAsync(Guid ownerId, PaginationParams pagination, string? status, CancellationToken cancellationToken = default);
    Task<Result<ApartmentDetailDto>> GetApartmentByIdAsync(Guid ownerId, Guid apartmentId, CancellationToken cancellationToken = default);
    Task<Result<ApartmentDto>> CreateApartmentAsync(Guid ownerId, CreateApartmentRequest request, CancellationToken cancellationToken = default);
    Task<Result<ApartmentDto>> UpdateApartmentAsync(Guid ownerId, Guid apartmentId, UpdateApartmentRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteApartmentAsync(Guid ownerId, Guid apartmentId, CancellationToken cancellationToken = default);
}
