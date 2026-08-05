using ApartmentRental.Domain.Common;
using ApartmentRental.Domain.Enums;

namespace ApartmentRental.Domain.Entities;

public class SmsLog : BaseEntity
{
    public string ToPhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public CommunicationStatus Status { get; set; } = CommunicationStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public Guid? RelatedRenterId { get; set; }
}
