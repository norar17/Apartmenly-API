namespace ApartmentRental.Application.Renters.DTOs;

public record RenterDto(
    Guid Id, string FullName, string Email, string PhoneNumber, string? Address,
    DateTime? MoveInDate, bool IsActive, string? CurrentApartmentNumber
);

public record RenterDetailDto(
    Guid Id, string FullName, string Email, string PhoneNumber, string? Address,
    string? EmergencyContactName, string? EmergencyContactPhone, DateTime? MoveInDate,
    bool IsActive, List<RenterLeaseSummaryDto> Leases
);

public record RenterLeaseSummaryDto(Guid LeaseId, string ApartmentNumber, DateTime StartDate, DateTime EndDate, string Status, decimal MonthlyRent);

public record CreateRenterRequest(
    string FullName, string Email, string PhoneNumber, string Password, string? Address,
    string? EmergencyContactName, string? EmergencyContactPhone, DateTime? MoveInDate
);

public record UpdateRenterRequest(
    string FullName, string Email, string PhoneNumber, string? Address,
    string? EmergencyContactName, string? EmergencyContactPhone, bool IsActive
);
