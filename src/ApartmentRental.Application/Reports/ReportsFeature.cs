using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApartmentRental.Application.Reports.DTOs
{
    public record ReportDateRangeRequest(DateTime From, DateTime To);
    public record RentCollectionReportRowDto(string ApartmentNumber, string RenterName, decimal AmountDue, decimal AmountPaid, string Status, DateTime? PaymentDate);
    public record OccupancyReportRowDto(string ApartmentNumber, string Status, string? CurrentRenter, DateTime? LeaseStart, DateTime? LeaseEnd);
    public record LatePaymentReportRowDto(string ApartmentNumber, string RenterName, decimal AmountDue, DateTime DueDate, int DaysLate);
    public record MaintenanceReportRowDto(string ApartmentNumber, string Title, string Priority, string Status, DateTime CreatedAt, DateTime? ResolvedAt);
}

namespace ApartmentRental.Application.Reports.Interfaces
{
    using ApartmentRental.Application.Reports.DTOs;

    public interface IReportService
    {
        Task<List<RentCollectionReportRowDto>> GetRentCollectionReportAsync(ReportDateRangeRequest range, CancellationToken cancellationToken = default);
        Task<List<OccupancyReportRowDto>> GetOccupancyReportAsync(CancellationToken cancellationToken = default);
        Task<List<LatePaymentReportRowDto>> GetLatePaymentReportAsync(CancellationToken cancellationToken = default);
        Task<List<MaintenanceReportRowDto>> GetMaintenanceReportAsync(ReportDateRangeRequest range, CancellationToken cancellationToken = default);
    }
}

namespace ApartmentRental.Application.Reports.Services
{
    using ApartmentRental.Application.Reports.DTOs;
    using ApartmentRental.Application.Reports.Interfaces;

    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<List<RentCollectionReportRowDto>> GetRentCollectionReportAsync(ReportDateRangeRequest range, CancellationToken cancellationToken = default)
        {
            var payments = await _unitOfWork.Repository<Payment>().Query()
                .Include(p => p.Lease).ThenInclude(l => l.Apartment)
                .Include(p => p.Lease).ThenInclude(l => l.Renter)
                .Where(p => p.DueDate >= range.From && p.DueDate <= range.To)
                .OrderBy(p => p.DueDate)
                .ToListAsync(cancellationToken);

            return payments.Select(p => new RentCollectionReportRowDto(
                p.Lease.Apartment.ApartmentNumber, p.Lease.Renter.FullName, p.Amount,
                p.Status == PaymentStatus.Paid ? p.Amount : 0, p.Status.ToString(), p.PaymentDate
            )).ToList();
        }

        public async Task<List<OccupancyReportRowDto>> GetOccupancyReportAsync(CancellationToken cancellationToken = default)
        {
            var apartments = await _unitOfWork.Repository<Apartment>().Query()
                .Include(a => a.Leases.Where(l => l.Status == LeaseStatus.Active))
                    .ThenInclude(l => l.Renter)
                .ToListAsync(cancellationToken);

            return apartments.Select(a =>
            {
                var lease = a.Leases.FirstOrDefault(l => l.Status == LeaseStatus.Active);
                return new OccupancyReportRowDto(a.ApartmentNumber, a.Status.ToString(), lease?.Renter.FullName, lease?.StartDate, lease?.EndDate);
            }).ToList();
        }

        public async Task<List<LatePaymentReportRowDto>> GetLatePaymentReportAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var payments = await _unitOfWork.Repository<Payment>().Query()
                .Include(p => p.Lease).ThenInclude(l => l.Apartment)
                .Include(p => p.Lease).ThenInclude(l => l.Renter)
                .Where(p => p.Status == PaymentStatus.Pending && p.DueDate < now)
                .OrderBy(p => p.DueDate)
                .ToListAsync(cancellationToken);

            return payments.Select(p => new LatePaymentReportRowDto(
                p.Lease.Apartment.ApartmentNumber, p.Lease.Renter.FullName, p.Amount, p.DueDate, (int)(now - p.DueDate).TotalDays
            )).ToList();
        }

        public async Task<List<MaintenanceReportRowDto>> GetMaintenanceReportAsync(ReportDateRangeRequest range, CancellationToken cancellationToken = default)
        {
            var requests = await _unitOfWork.Repository<MaintenanceRequest>().Query()
                .Include(m => m.Apartment)
                .Where(m => m.CreatedAt >= range.From && m.CreatedAt <= range.To)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync(cancellationToken);

            return requests.Select(m => new MaintenanceReportRowDto(
                m.Apartment.ApartmentNumber, m.Title, m.Priority.ToString(), m.Status.ToString(), m.CreatedAt, m.ResolvedAt
            )).ToList();
        }
    }
}
