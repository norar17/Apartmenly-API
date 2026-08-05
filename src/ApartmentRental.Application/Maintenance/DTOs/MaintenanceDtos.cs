using ApartmentRental.Domain.Enums;

namespace ApartmentRental.Application.Maintenance.DTOs;

public record MaintenanceRequestDto(
    Guid Id, Guid ApartmentId, string ApartmentNumber, Guid RenterId, string RenterName,
    string Title, string Description, MaintenancePriority Priority, MaintenanceStatus Status,
    string? OwnerNotes, DateTime CreatedAt, DateTime? ResolvedAt
);

public record CreateMaintenanceRequestRequest(Guid ApartmentId, string Title, string Description, MaintenancePriority Priority);

public record UpdateMaintenanceStatusRequest(MaintenanceStatus Status, string? OwnerNotes);
