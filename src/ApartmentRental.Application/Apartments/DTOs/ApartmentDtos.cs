using ApartmentRental.Domain.Enums;

namespace ApartmentRental.Application.Apartments.DTOs;

public record ApartmentDto(
    Guid Id, string ApartmentNumber, int Floor, string? Description,
    decimal MonthlyRent, decimal Deposit, ApartmentStatus Status,
    decimal? SizeSqm, int Bedrooms, int Bathrooms,
    IReadOnlyList<string> PhotoUrls, DateTime CreatedAt
);

public record ApartmentDetailDto(
    Guid Id, string ApartmentNumber, int Floor, string? Description,
    decimal MonthlyRent, decimal Deposit, ApartmentStatus Status,
    decimal? SizeSqm, int Bedrooms, int Bathrooms,
    IReadOnlyList<string> PhotoUrls, CurrentTenantDto? CurrentTenant, DateTime CreatedAt
);

public record CurrentTenantDto(Guid RenterId, string FullName, Guid LeaseId, DateTime LeaseStart, DateTime LeaseEnd);

public record CreateApartmentRequest(
    string ApartmentNumber, int Floor, string? Description, decimal MonthlyRent, decimal Deposit,
    decimal? SizeSqm, int Bedrooms, int Bathrooms, List<string>? PhotoUrls
);

public record UpdateApartmentRequest(
    string ApartmentNumber, int Floor, string? Description, decimal MonthlyRent, decimal Deposit,
    ApartmentStatus Status, decimal? SizeSqm, int Bedrooms, int Bathrooms
);
