using ApartmentRental.Domain.Enums;

namespace ApartmentRental.Application.Payments.DTOs;

public record PaymentDto(
    Guid Id, Guid LeaseId, string ApartmentNumber, string RenterName, string ReceiptNumber,
    decimal Amount, DateTime? PaymentDate, DateTime DueDate, PaymentMethod PaymentMethod,
    PaymentStatus Status, string? Notes, string? ForMonth
);

public record CreatePaymentRequest(Guid LeaseId, decimal Amount, DateTime DueDate, string? ForMonth, PaymentMethod PaymentMethod, string? Notes);

public record MarkPaymentPaidRequest(DateTime PaymentDate, PaymentMethod PaymentMethod, string? Notes);
