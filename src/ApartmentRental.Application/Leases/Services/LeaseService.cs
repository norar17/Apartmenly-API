using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Leases.DTOs;
using ApartmentRental.Application.Leases.Interfaces;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;
using ApartmentRental.Shared;
using Microsoft.EntityFrameworkCore;

namespace ApartmentRental.Application.Leases.Services;

public class LeaseService : ILeaseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActivityLogger _activityLogger;
    private readonly INotificationService _notificationService;

    public LeaseService(IUnitOfWork unitOfWork, IActivityLogger activityLogger, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<LeaseDto>> GetLeasesAsync(PaginationParams pagination, string? status, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Lease>().Query()
            .Include(l => l.Apartment)
            .Include(l => l.Renter)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LeaseStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(l => l.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            var term = pagination.Search.Trim().ToLower();
            query = query.Where(l => l.Apartment.ApartmentNumber.ToLower().Contains(term) || l.Renter.FullName.ToLower().Contains(term));
        }

        query = pagination.SortDescending ? query.OrderByDescending(l => l.StartDate) : query.OrderBy(l => l.StartDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<LeaseDto>.Create(entities.Select(MapToDto), totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<Result<LeaseDto>> GetLeaseByIdAsync(Guid leaseId, CancellationToken cancellationToken = default)
    {
        var lease = await _unitOfWork.Repository<Lease>().Query()
            .Include(l => l.Apartment)
            .Include(l => l.Renter)
            .FirstOrDefaultAsync(l => l.Id == leaseId, cancellationToken);

        return lease is null
            ? Result.NotFound<LeaseDto>("Lease", leaseId)
            : Result.Success(MapToDto(lease));
    }

    public async Task<Result<LeaseDto>> GetMyActiveLeaseAsync(Guid renterId, CancellationToken cancellationToken = default)
    {
        var lease = await _unitOfWork.Repository<Lease>().Query()
            .Include(l => l.Apartment)
            .Include(l => l.Renter)
            .Where(l => l.RenterId == renterId && l.Status == LeaseStatus.Active)
            .OrderByDescending(l => l.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        return lease is null
            ? Result.Failure<LeaseDto>("You don't have an active lease right now.", "NO_ACTIVE_LEASE")
            : Result.Success(MapToDto(lease));
    }

    public async Task<Result<LeaseDto>> CreateLeaseAsync(CreateLeaseRequest request, CancellationToken cancellationToken = default)
    {
        var apartments = _unitOfWork.Repository<Apartment>();
        var apartment = await apartments.Query(asNoTracking: false).FirstOrDefaultAsync(a => a.Id == request.ApartmentId, cancellationToken);
        if (apartment is null)
        {
            return Result.NotFound<LeaseDto>("Apartment", request.ApartmentId);
        }

        if (apartment.Status is ApartmentStatus.Occupied or ApartmentStatus.Maintenance)
        {
            return Result.Failure<LeaseDto>($"Apartment is currently {apartment.Status} and cannot be leased.", "APARTMENT_UNAVAILABLE");
        }

        var renter = await _unitOfWork.Repository<Renter>().GetByIdAsync(request.RenterId, cancellationToken);
        if (renter is null)
        {
            return Result.NotFound<LeaseDto>("Renter", request.RenterId);
        }

        if (request.EndDate <= request.StartDate)
        {
            return Result.Failure<LeaseDto>("Lease end date must be after the start date.", "INVALID_DATE_RANGE");
        }

        var lease = new Lease
        {
            ApartmentId = request.ApartmentId,
            RenterId = request.RenterId,
            MonthlyRent = request.MonthlyRent,
            DueDay = request.DueDay,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Notes = request.Notes,
            Status = LeaseStatus.Active
        };

        apartment.Status = ApartmentStatus.Occupied;
        apartments.Update(apartment);

        await _unitOfWork.Repository<Lease>().AddAsync(lease, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync("Lease.Created", $"{renter.FullName} was leased apartment {apartment.ApartmentNumber}.", nameof(Lease), lease.Id, cancellationToken);
        await _notificationService.NotifyAsync(renter.Id, UserRole.Renter, "Lease started",
            $"Your lease for apartment {apartment.ApartmentNumber} is now active.", NotificationType.General, cancellationToken: cancellationToken);

        lease.Apartment = apartment;
        lease.Renter = renter;
        return Result.Success(MapToDto(lease));
    }

    public async Task<Result<LeaseDto>> RenewLeaseAsync(Guid leaseId, RenewLeaseRequest request, CancellationToken cancellationToken = default)
    {
        var leases = _unitOfWork.Repository<Lease>();
        var lease = await leases.Query(asNoTracking: false)
            .Include(l => l.Apartment)
            .Include(l => l.Renter)
            .FirstOrDefaultAsync(l => l.Id == leaseId, cancellationToken);

        if (lease is null)
        {
            return Result.NotFound<LeaseDto>("Lease", leaseId);
        }

        if (request.NewEndDate <= lease.StartDate)
        {
            return Result.Failure<LeaseDto>("New end date must be after the lease start date.", "INVALID_DATE_RANGE");
        }

        lease.EndDate = request.NewEndDate;
        if (request.NewMonthlyRent.HasValue)
        {
            lease.MonthlyRent = request.NewMonthlyRent.Value;
        }
        lease.Status = LeaseStatus.Renewed;
        lease.UpdatedAt = DateTime.UtcNow;

        leases.Update(lease);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync("Lease.Renewed", $"Lease for apartment {lease.Apartment.ApartmentNumber} was renewed until {lease.EndDate:yyyy-MM-dd}.", nameof(Lease), lease.Id, cancellationToken);

        return Result.Success(MapToDto(lease));
    }

    public async Task<Result> TerminateLeaseAsync(Guid leaseId, TerminateLeaseRequest request, CancellationToken cancellationToken = default)
    {
        var leases = _unitOfWork.Repository<Lease>();
        var lease = await leases.Query(asNoTracking: false)
            .Include(l => l.Apartment)
            .Include(l => l.Renter)
            .FirstOrDefaultAsync(l => l.Id == leaseId, cancellationToken);

        if (lease is null)
        {
            return Result.NotFound("Lease", leaseId);
        }

        lease.Status = LeaseStatus.Terminated;
        lease.TerminatedAt = request.EffectiveDate ?? DateTime.UtcNow;
        lease.TerminationReason = request.Reason;
        lease.UpdatedAt = DateTime.UtcNow;
        lease.Apartment.Status = ApartmentStatus.Available;

        leases.Update(lease);
        _unitOfWork.Repository<Apartment>().Update(lease.Apartment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync("Lease.Terminated", $"Lease for apartment {lease.Apartment.ApartmentNumber} was terminated.", nameof(Lease), lease.Id, cancellationToken);
        await _notificationService.NotifyAsync(lease.RenterId, UserRole.Renter, "Lease terminated",
            $"Your lease for apartment {lease.Apartment.ApartmentNumber} has been terminated.", NotificationType.LeaseTerminated, cancellationToken: cancellationToken);

        return Result.Success();
    }

    public async Task<Result> MoveOutAsync(Guid leaseId, CancellationToken cancellationToken = default)
    {
        var leases = _unitOfWork.Repository<Lease>();
        var lease = await leases.Query(asNoTracking: false)
            .Include(l => l.Apartment)
            .FirstOrDefaultAsync(l => l.Id == leaseId, cancellationToken);

        if (lease is null)
        {
            return Result.NotFound("Lease", leaseId);
        }

        lease.Status = LeaseStatus.Expired;
        lease.Apartment.Status = ApartmentStatus.Available;
        lease.UpdatedAt = DateTime.UtcNow;

        leases.Update(lease);
        _unitOfWork.Repository<Apartment>().Update(lease.Apartment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync("Lease.MoveOut", $"Tenant moved out of apartment {lease.Apartment.ApartmentNumber}.", nameof(Lease), lease.Id, cancellationToken);

        return Result.Success();
    }

    private static LeaseDto MapToDto(Lease l) => new(
        l.Id, l.ApartmentId, l.Apartment.ApartmentNumber, l.RenterId, l.Renter.FullName,
        l.MonthlyRent, l.DueDay, l.StartDate, l.EndDate, l.Status.ToString());
}
