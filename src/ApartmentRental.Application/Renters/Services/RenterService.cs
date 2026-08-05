using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Renters.DTOs;
using ApartmentRental.Application.Renters.Interfaces;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;
using ApartmentRental.Shared;
using Microsoft.EntityFrameworkCore;

namespace ApartmentRental.Application.Renters.Services;

public class RenterService : IRenterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IActivityLogger _activityLogger;

    public RenterService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _activityLogger = activityLogger;
    }

    public async Task<PagedResult<RenterDto>> GetRentersAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Renter>().Query()
            .Include(r => r.Leases.Where(l => l.Status == LeaseStatus.Active))
                .ThenInclude(l => l.Apartment)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            var term = pagination.Search.Trim().ToLower();
            query = query.Where(r => r.FullName.ToLower().Contains(term) || r.Email.ToLower().Contains(term) || r.PhoneNumber.Contains(term));
        }

        query = pagination.SortDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.FullName);

        var totalCount = await query.CountAsync(cancellationToken);

        var renters = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var items = renters.Select(r =>
        {
            var activeLease = r.Leases.FirstOrDefault(l => l.Status == LeaseStatus.Active);
            return new RenterDto(r.Id, r.FullName, r.Email, r.PhoneNumber, r.Address, r.MoveInDate, r.IsActive,
                activeLease?.Apartment.ApartmentNumber);
        });

        return PagedResult<RenterDto>.Create(items, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<Result<RenterDetailDto>> GetRenterByIdAsync(Guid renterId, CancellationToken cancellationToken = default)
    {
        var renter = await _unitOfWork.Repository<Renter>().Query()
            .Include(r => r.Leases)
                .ThenInclude(l => l.Apartment)
            .FirstOrDefaultAsync(r => r.Id == renterId, cancellationToken);

        if (renter is null)
        {
            return Result.NotFound<RenterDetailDto>("Renter", renterId);
        }

        var leases = renter.Leases
            .OrderByDescending(l => l.StartDate)
            .Select(l => new RenterLeaseSummaryDto(l.Id, l.Apartment.ApartmentNumber, l.StartDate, l.EndDate, l.Status.ToString(), l.MonthlyRent))
            .ToList();

        var dto = new RenterDetailDto(renter.Id, renter.FullName, renter.Email, renter.PhoneNumber, renter.Address,
            renter.EmergencyContactName, renter.EmergencyContactPhone, renter.MoveInDate, renter.IsActive, leases);

        return Result.Success(dto);
    }

    public async Task<Result<RenterDto>> CreateRenterAsync(CreateRenterRequest request, CancellationToken cancellationToken = default)
    {
        var renters = _unitOfWork.Repository<Renter>();
        var emailTaken = await renters.ExistsAsync(r => r.Email == request.Email, cancellationToken);
        if (emailTaken)
        {
            return Result.Failure<RenterDto>("A renter with this email already exists.", "EMAIL_TAKEN");
        }

        var renter = new Renter
        {
            FullName = request.FullName,
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Address = request.Address,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            MoveInDate = request.MoveInDate
        };

        await renters.AddAsync(renter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync("Renter.Created", $"{renter.FullName} was added as a renter.", nameof(Renter), renter.Id, cancellationToken);

        return Result.Success(new RenterDto(renter.Id, renter.FullName, renter.Email, renter.PhoneNumber, renter.Address, renter.MoveInDate, renter.IsActive, null));
    }

    public async Task<Result<RenterDto>> UpdateRenterAsync(Guid renterId, UpdateRenterRequest request, CancellationToken cancellationToken = default)
    {
        var renters = _unitOfWork.Repository<Renter>();
        var renter = await renters.Query(asNoTracking: false).FirstOrDefaultAsync(r => r.Id == renterId, cancellationToken);
        if (renter is null)
        {
            return Result.NotFound<RenterDto>("Renter", renterId);
        }

        renter.FullName = request.FullName;
        renter.Email = request.Email.Trim().ToLowerInvariant();
        renter.PhoneNumber = request.PhoneNumber;
        renter.Address = request.Address;
        renter.EmergencyContactName = request.EmergencyContactName;
        renter.EmergencyContactPhone = request.EmergencyContactPhone;
        renter.IsActive = request.IsActive;
        renter.UpdatedAt = DateTime.UtcNow;

        renters.Update(renter);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RenterDto(renter.Id, renter.FullName, renter.Email, renter.PhoneNumber, renter.Address, renter.MoveInDate, renter.IsActive, null));
    }

    public async Task<Result> DeleteRenterAsync(Guid renterId, CancellationToken cancellationToken = default)
    {
        var renters = _unitOfWork.Repository<Renter>();
        var renter = await renters.Query(asNoTracking: false)
            .Include(r => r.Leases)
            .FirstOrDefaultAsync(r => r.Id == renterId, cancellationToken);

        if (renter is null)
        {
            return Result.NotFound("Renter", renterId);
        }

        if (renter.Leases.Any(l => l.Status == LeaseStatus.Active))
        {
            return Result.Failure("Cannot delete a renter with an active lease.", "HAS_ACTIVE_LEASE");
        }

        renters.SoftDelete(renter);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
