using ApartmentRental.Domain.Common;
using ApartmentRental.Domain.Enums;

namespace ApartmentRental.Domain.Entities;

public class Apartment : BaseEntity
{
    public Guid OwnerId { get; set; }
    public string ApartmentNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public string? Description { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal Deposit { get; set; }
    public ApartmentStatus Status { get; set; } = ApartmentStatus.Available;
    public decimal? SizeSqm { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }

    public Owner Owner { get; set; } = null!;
    public ICollection<ApartmentPhoto> Photos { get; set; } = new List<ApartmentPhoto>();
    public ICollection<Lease> Leases { get; set; } = new List<Lease>();
    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
}
