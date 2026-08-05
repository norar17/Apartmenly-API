using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Maintenance.DTOs;
using ApartmentRental.Shared;

namespace ApartmentRental.Application.Maintenance.Interfaces;

public interface IMaintenanceService
{
    Task<PagedResult<MaintenanceRequestDto>> GetRequestsAsync(PaginationParams pagination, string? status, string? priority, CancellationToken cancellationToken = default);
    Task<PagedResult<MaintenanceRequestDto>> GetRequestsForRenterAsync(Guid renterId, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<Result<MaintenanceRequestDto>> CreateRequestAsync(Guid renterId, CreateMaintenanceRequestRequest request, CancellationToken cancellationToken = default);
    Task<Result<MaintenanceRequestDto>> UpdateStatusAsync(Guid requestId, UpdateMaintenanceStatusRequest request, CancellationToken cancellationToken = default);
}
