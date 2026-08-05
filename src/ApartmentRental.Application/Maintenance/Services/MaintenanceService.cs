using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Maintenance.DTOs;
using ApartmentRental.Application.Maintenance.Interfaces;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;
using ApartmentRental.Shared;
using Microsoft.EntityFrameworkCore;

namespace ApartmentRental.Application.Maintenance.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActivityLogger _activityLogger;
    private readonly INotificationService _notificationService;

    public MaintenanceService(IUnitOfWork unitOfWork, IActivityLogger activityLogger, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<MaintenanceRequestDto>> GetRequestsAsync(PaginationParams pagination, string? status, string? priority, CancellationToken cancellationToken = default)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<MaintenanceStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(m => m.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<MaintenancePriority>(priority, true, out var parsedPriority))
        {
            query = query.Where(m => m.Priority == parsedPriority);
        }

        query = query.OrderByDescending(m => m.Priority).ThenByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<MaintenanceRequestDto>.Create(entities.Select(MapToDto), totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<PagedResult<MaintenanceRequestDto>> GetRequestsForRenterAsync(Guid renterId, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = BaseQuery().Where(m => m.RenterId == renterId).OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<MaintenanceRequestDto>.Create(entities.Select(MapToDto), totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<Result<MaintenanceRequestDto>> CreateRequestAsync(Guid renterId, CreateMaintenanceRequestRequest request, CancellationToken cancellationToken = default)
    {
        var apartment = await _unitOfWork.Repository<Apartment>().Query()
            .Include(a => a.Owner)
            .FirstOrDefaultAsync(a => a.Id == request.ApartmentId, cancellationToken);

        if (apartment is null)
        {
            return Result.NotFound<MaintenanceRequestDto>("Apartment", request.ApartmentId);
        }

        var renter = await _unitOfWork.Repository<Renter>().GetByIdAsync(renterId, cancellationToken);
        if (renter is null)
        {
            return Result.NotFound<MaintenanceRequestDto>("Renter", renterId);
        }

        var maintenanceRequest = new MaintenanceRequest
        {
            ApartmentId = request.ApartmentId,
            RenterId = renterId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = MaintenanceStatus.Pending
        };

        await _unitOfWork.Repository<MaintenanceRequest>().AddAsync(maintenanceRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync("Maintenance.Created",
            $"{renter.FullName} submitted a maintenance request for apartment {apartment.ApartmentNumber}.",
            nameof(MaintenanceRequest), maintenanceRequest.Id, cancellationToken);

        await _notificationService.NotifyAsync(apartment.OwnerId, UserRole.Owner, "New maintenance request",
            $"{renter.FullName} reported: {request.Title}", NotificationType.NewMaintenanceRequest, cancellationToken: cancellationToken);

        maintenanceRequest.Apartment = apartment;
        maintenanceRequest.Renter = renter;
        return Result.Success(MapToDto(maintenanceRequest));
    }

    public async Task<Result<MaintenanceRequestDto>> UpdateStatusAsync(Guid requestId, UpdateMaintenanceStatusRequest request, CancellationToken cancellationToken = default)
    {
        var requests = _unitOfWork.Repository<MaintenanceRequest>();
        var maintenanceRequest = await requests.Query(asNoTracking: false)
            .Include(m => m.Apartment)
            .Include(m => m.Renter)
            .FirstOrDefaultAsync(m => m.Id == requestId, cancellationToken);

        if (maintenanceRequest is null)
        {
            return Result.NotFound<MaintenanceRequestDto>("Maintenance request", requestId);
        }

        maintenanceRequest.Status = request.Status;
        maintenanceRequest.OwnerNotes = request.OwnerNotes;
        maintenanceRequest.UpdatedAt = DateTime.UtcNow;
        if (request.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled)
        {
            maintenanceRequest.ResolvedAt = DateTime.UtcNow;
        }

        requests.Update(maintenanceRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyAsync(maintenanceRequest.RenterId, UserRole.Renter, "Maintenance update",
            $"Your request \"{maintenanceRequest.Title}\" is now {maintenanceRequest.Status}.",
            NotificationType.MaintenanceUpdate, cancellationToken: cancellationToken);

        return Result.Success(MapToDto(maintenanceRequest));
    }

    private IQueryable<MaintenanceRequest> BaseQuery() => _unitOfWork.Repository<MaintenanceRequest>().Query()
        .Include(m => m.Apartment)
        .Include(m => m.Renter);

    private static MaintenanceRequestDto MapToDto(MaintenanceRequest m) => new(
        m.Id, m.ApartmentId, m.Apartment.ApartmentNumber, m.RenterId, m.Renter.FullName,
        m.Title, m.Description, m.Priority, m.Status, m.OwnerNotes, m.CreatedAt, m.ResolvedAt);
}
