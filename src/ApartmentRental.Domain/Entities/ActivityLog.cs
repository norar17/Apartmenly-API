using ApartmentRental.Domain.Common;

namespace ApartmentRental.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public Guid? ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // e.g. "Apartment.Created"
    public string Description { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
}
