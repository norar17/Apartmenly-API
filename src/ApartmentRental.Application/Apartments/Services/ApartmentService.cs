using ApartmentRental.Application.Apartments.DTOs;
using ApartmentRental.Application.Apartments.Interfaces;
using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Application.Common.Models;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;
using ApartmentRental.Shared;
using Microsoft.EntityFrameworkCore;

namespace ApartmentRental.Application.Apartments.Services;

public class ApartmentService : IApartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActivityLogger _activityLogger;

    public ApartmentService(IUnitOfWork unitOfWork, IActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
    }

    public async Task<PagedResult<ApartmentDto>> GetApartmentsAsync(Guid ownerId, PaginationParams pagination, string? status, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Apartment>().Query()
            .Include(a => a.Photos)
            .Where(a => a.OwnerId == ownerId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ApartmentStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(a => a.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            var term = pagination.Search.Trim().ToLower();
            query = query.Where(a => a.ApartmentNumber.ToLower().Contains(term) || (a.Description != null && a.Description.ToLower().Contains(term)));
        }

        query = pagination.SortBy?.ToLower() switch
        {
            "rent" => pagination.SortDescending ? query.OrderByDescending(a => a.MonthlyRent) : query.OrderBy(a => a.MonthlyRent),
            "floor" => pagination.SortDescending ? query.OrderByDescending(a => a.Floor) : query.OrderBy(a => a.Floor),
            _ => pagination.SortDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.ApartmentNumber)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<ApartmentDto>.Create(entities.Select(MapToDto), totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<Result<ApartmentDetailDto>> GetApartmentByIdAsync(Guid ownerId, Guid apartmentId, CancellationToken cancellationToken = default)
    {
        var apartment = await _unitOfWork.Repository<Apartment>().Query()
            .Include(a => a.Photos)
            .Include(a => a.Leases.Where(l => l.Status == LeaseStatus.Active))
                .ThenInclude(l => l.Renter)
            .FirstOrDefaultAsync(a => a.Id == apartmentId && a.OwnerId == ownerId, cancellationToken);

        if (apartment is null)
        {
            return Result.NotFound<ApartmentDetailDto>("Apartment", apartmentId);
        }

        var activeLease = apartment.Leases.FirstOrDefault(l => l.Status == LeaseStatus.Active);
        CurrentTenantDto? tenant = activeLease is null
            ? null
            : new CurrentTenantDto(activeLease.RenterId, activeLease.Renter.FullName, activeLease.Id, activeLease.StartDate, activeLease.EndDate);

        var dto = new ApartmentDetailDto(
            apartment.Id, apartment.ApartmentNumber, apartment.Floor, apartment.Description,
            apartment.MonthlyRent, apartment.Deposit, apartment.Status, apartment.SizeSqm,
            apartment.Bedrooms, apartment.Bathrooms,
            apartment.Photos.OrderBy(p => p.SortOrder).Select(p => p.Url).ToList(),
            tenant, apartment.CreatedAt);

        return Result.Success(dto);
    }

    public async Task<Result<ApartmentDto>> CreateApartmentAsync(Guid ownerId, CreateApartmentRequest request, CancellationToken cancellationToken = default)
    {
        var apartments = _unitOfWork.Repository<Apartment>();
        var duplicateExists = await apartments.ExistsAsync(
            a => a.OwnerId == ownerId && a.ApartmentNumber == request.ApartmentNumber, cancellationToken);

        if (duplicateExists)
        {
            return Result.Failure<ApartmentDto>("An apartment with this number already exists.", "DUPLICATE");
        }

        var apartment = new Apartment
        {
            OwnerId = ownerId,
            ApartmentNumber = request.ApartmentNumber,
            Floor = request.Floor,
            Description = request.Description,
            MonthlyRent = request.MonthlyRent,
            Deposit = request.Deposit,
            SizeSqm = request.SizeSqm,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            Status = ApartmentStatus.Available
        };

        if (request.PhotoUrls is { Count: > 0 })
        {
            for (var i = 0; i < request.PhotoUrls.Count; i++)
            {
                apartment.Photos.Add(new ApartmentPhoto { Url = request.PhotoUrls[i], SortOrder = i, IsPrimary = i == 0 });
            }
        }

        await apartments.AddAsync(apartment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync("Apartment.Created", $"Apartment {apartment.ApartmentNumber} was added.", nameof(Apartment), apartment.Id, cancellationToken);

        return Result.Success(MapToDto(apartment));
    }

    public async Task<Result<ApartmentDto>> UpdateApartmentAsync(Guid ownerId, Guid apartmentId, UpdateApartmentRequest request, CancellationToken cancellationToken = default)
    {
        var apartments = _unitOfWork.Repository<Apartment>();
        var apartment = await apartments.Query(asNoTracking: false)
            .Include(a => a.Photos)
            .FirstOrDefaultAsync(a => a.Id == apartmentId && a.OwnerId == ownerId, cancellationToken);

        if (apartment is null)
        {
            return Result.NotFound<ApartmentDto>("Apartment", apartmentId);
        }

        apartment.ApartmentNumber = request.ApartmentNumber;
        apartment.Floor = request.Floor;
        apartment.Description = request.Description;
        apartment.MonthlyRent = request.MonthlyRent;
        apartment.Deposit = request.Deposit;
        apartment.Status = request.Status;
        apartment.SizeSqm = request.SizeSqm;
        apartment.Bedrooms = request.Bedrooms;
        apartment.Bathrooms = request.Bathrooms;
        apartment.UpdatedAt = DateTime.UtcNow;

        apartments.Update(apartment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync("Apartment.Updated", $"Apartment {apartment.ApartmentNumber} was updated.", nameof(Apartment), apartment.Id, cancellationToken);

        return Result.Success(MapToDto(apartment));
    }

    public async Task<Result> DeleteApartmentAsync(Guid ownerId, Guid apartmentId, CancellationToken cancellationToken = default)
    {
        var apartments = _unitOfWork.Repository<Apartment>();
        var apartment = await apartments.Query(asNoTracking: false)
            .Include(a => a.Leases)
            .FirstOrDefaultAsync(a => a.Id == apartmentId && a.OwnerId == ownerId, cancellationToken);

        if (apartment is null)
        {
            return Result.NotFound("Apartment", apartmentId);
        }

        if (apartment.Leases.Any(l => l.Status == LeaseStatus.Active))
        {
            return Result.Failure("Cannot delete an apartment with an active lease.", "HAS_ACTIVE_LEASE");
        }

        apartments.SoftDelete(apartment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync("Apartment.Deleted", $"Apartment {apartment.ApartmentNumber} was removed.", nameof(Apartment), apartment.Id, cancellationToken);

        return Result.Success();
    }

    private static ApartmentDto MapToDto(Apartment a) => new(
        a.Id, a.ApartmentNumber, a.Floor, a.Description, a.MonthlyRent, a.Deposit, a.Status,
        a.SizeSqm, a.Bedrooms, a.Bathrooms,
        a.Photos.OrderBy(p => p.SortOrder).Select(p => p.Url).ToList(), a.CreatedAt);
}
