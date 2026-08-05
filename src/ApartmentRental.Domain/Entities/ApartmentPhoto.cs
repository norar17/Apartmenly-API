using ApartmentRental.Domain.Common;

namespace ApartmentRental.Domain.Entities;

public class ApartmentPhoto : BaseEntity
{
    public Guid ApartmentId { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }

    public Apartment Apartment { get; set; } = null!;
}
