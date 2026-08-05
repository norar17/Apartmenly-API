using ApartmentRental.Domain.Common;
using ApartmentRental.Domain.Enums;

namespace ApartmentRental.Domain.Entities;

public class MaintenanceRequest : BaseEntity
{
    public Guid ApartmentId { get; set; }
    public Guid RenterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MaintenancePriority Priority { get; set; } = MaintenancePriority.Medium;
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Pending;
    public string? OwnerNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public Apartment Apartment { get; set; } = null!;
    public Renter Renter { get; set; } = null!;
}
