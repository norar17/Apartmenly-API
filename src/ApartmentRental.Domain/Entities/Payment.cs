using ApartmentRental.Domain.Common;
using ApartmentRental.Domain.Enums;

namespace ApartmentRental.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid LeaseId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime DueDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? Notes { get; set; }
    public string? ForMonth { get; set; } // e.g. "2026-08"

    public Lease Lease { get; set; } = null!;
}
