using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Application.Common.Models;
using ApartmentRental.Application.Payments.DTOs;
using ApartmentRental.Application.Payments.Interfaces;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;
using ApartmentRental.Shared;
using Microsoft.EntityFrameworkCore;

namespace ApartmentRental.Application.Payments.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActivityLogger _activityLogger;
    private readonly INotificationService _notificationService;

    public PaymentService(IUnitOfWork unitOfWork, IActivityLogger activityLogger, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<PaymentDto>> GetPaymentsAsync(PaginationParams pagination, string? status, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(p => p.Status == parsedStatus);
        }

        if (from.HasValue) query = query.Where(p => p.DueDate >= from.Value);
        if (to.HasValue) query = query.Where(p => p.DueDate <= to.Value);

        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            var term = pagination.Search.Trim().ToLower();
            query = query.Where(p => p.Lease.Renter.FullName.ToLower().Contains(term)
                || p.Lease.Apartment.ApartmentNumber.ToLower().Contains(term)
                || p.ReceiptNumber.ToLower().Contains(term));
        }

        query = pagination.SortDescending ? query.OrderByDescending(p => p.DueDate) : query.OrderBy(p => p.DueDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<PaymentDto>.Create(entities.Select(MapToDto), totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<PagedResult<PaymentDto>> GetPaymentsForRenterAsync(Guid renterId, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = BaseQuery().Where(p => p.Lease.RenterId == renterId).OrderByDescending(p => p.DueDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<PaymentDto>.Create(entities.Select(MapToDto), totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<Result<PaymentDto>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var lease = await _unitOfWork.Repository<Lease>().Query()
            .Include(l => l.Apartment)
            .Include(l => l.Renter)
            .FirstOrDefaultAsync(l => l.Id == request.LeaseId, cancellationToken);

        if (lease is null)
        {
            return Result.NotFound<PaymentDto>("Lease", request.LeaseId);
        }

        var payment = new Payment
        {
            LeaseId = request.LeaseId,
            ReceiptNumber = GenerateReceiptNumber(),
            Amount = request.Amount,
            DueDate = request.DueDate,
            ForMonth = request.ForMonth,
            PaymentMethod = request.PaymentMethod,
            Notes = request.Notes,
            Status = PaymentStatus.Pending
        };

        await _unitOfWork.Repository<Payment>().AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        payment.Lease = lease;
        return Result.Success(MapToDto(payment));
    }

    public async Task<Result<PaymentDto>> MarkAsPaidAsync(Guid paymentId, MarkPaymentPaidRequest request, CancellationToken cancellationToken = default)
    {
        var payments = _unitOfWork.Repository<Payment>();
        var payment = await payments.Query(asNoTracking: false)
            .Include(p => p.Lease).ThenInclude(l => l.Apartment)
            .Include(p => p.Lease).ThenInclude(l => l.Renter)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment is null)
        {
            return Result.NotFound<PaymentDto>("Payment", paymentId);
        }

        payment.Status = PaymentStatus.Paid;
        payment.PaymentDate = request.PaymentDate;
        payment.PaymentMethod = request.PaymentMethod;
        payment.Notes = request.Notes ?? payment.Notes;
        payment.UpdatedAt = DateTime.UtcNow;

        payments.Update(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync("Payment.Marked Paid",
            $"Payment of {payment.Amount:C} for {payment.Lease.Apartment.ApartmentNumber} was recorded as paid.",
            nameof(Payment), payment.Id, cancellationToken);

        await _notificationService.NotifyAsync(payment.Lease.RenterId, UserRole.Renter, "Payment received",
            $"We received your payment of {payment.Amount:C}. Receipt #{payment.ReceiptNumber}.",
            NotificationType.PaymentReceived, cancellationToken: cancellationToken);

        return Result.Success(MapToDto(payment));
    }

    public async Task<Result> CancelPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payments = _unitOfWork.Repository<Payment>();
        var payment = await payments.Query(asNoTracking: false).FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);
        if (payment is null)
        {
            return Result.NotFound("Payment", paymentId);
        }

        payment.Status = PaymentStatus.Cancelled;
        payment.UpdatedAt = DateTime.UtcNow;
        payments.Update(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private IQueryable<Payment> BaseQuery() => _unitOfWork.Repository<Payment>().Query()
        .Include(p => p.Lease).ThenInclude(l => l.Apartment)
        .Include(p => p.Lease).ThenInclude(l => l.Renter);

    private static string GenerateReceiptNumber() => $"RCT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";

    private static PaymentDto MapToDto(Payment p) => new(
        p.Id, p.LeaseId, p.Lease.Apartment.ApartmentNumber, p.Lease.Renter.FullName, p.ReceiptNumber,
        p.Amount, p.PaymentDate, p.DueDate, p.PaymentMethod, p.Status, p.Notes, p.ForMonth);
}
