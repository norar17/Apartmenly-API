using ApartmentRental.Domain.Common;
using ApartmentRental.Domain.Enums;

namespace ApartmentRental.Domain.Entities;

public class Lease : BaseEntity
{
    public Guid ApartmentId { get; set; }
    public Guid RenterId { get; set; }
    public decimal MonthlyRent { get; set; }
    public int DueDay { get; set; } // day of month, 1-28 recommended
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LeaseStatus Status { get; set; } = LeaseStatus.Active;
    public DateTime? TerminatedAt { get; set; }
    public string? TerminationReason { get; set; }
    public string? Notes { get; set; }

    public Apartment Apartment { get; set; } = null!;
    public Renter Renter { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
