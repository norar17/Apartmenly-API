using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Payments.DTOs;
using ApartmentRental.Shared;

namespace ApartmentRental.Application.Payments.Interfaces;

public interface IPaymentService
{
    Task<PagedResult<PaymentDto>> GetPaymentsAsync(PaginationParams pagination, string? status, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<PagedResult<PaymentDto>> GetPaymentsForRenterAsync(Guid renterId, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> MarkAsPaidAsync(Guid paymentId, MarkPaymentPaidRequest request, CancellationToken cancellationToken = default);
    Task<Result> CancelPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
}
