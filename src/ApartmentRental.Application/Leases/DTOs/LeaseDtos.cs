namespace ApartmentRental.Application.Leases.DTOs;

public record LeaseDto(
    Guid Id, Guid ApartmentId, string ApartmentNumber, Guid RenterId, string RenterName,
    decimal MonthlyRent, int DueDay, DateTime StartDate, DateTime EndDate, string Status
);

public record CreateLeaseRequest(
    Guid ApartmentId, Guid RenterId, decimal MonthlyRent, int DueDay,
    DateTime StartDate, DateTime EndDate, string? Notes
);

public record RenewLeaseRequest(DateTime NewEndDate, decimal? NewMonthlyRent);

public record TerminateLeaseRequest(string Reason, DateTime? EffectiveDate);
