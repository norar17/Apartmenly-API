using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApartmentRental.Application.Dashboard.DTOs
{
    public record DashboardSummaryDto(
        int TotalApartments, int OccupiedApartments, int AvailableApartments, int MaintenanceApartments,
        decimal MonthlyRevenue, decimal PendingPaymentsAmount, int PendingPaymentsCount,
        int LatePaymentsCount, int OpenMaintenanceRequests
    );

    public record MonthlyRevenuePointDto(string Month, decimal Revenue);
    public record OccupancyPointDto(string Month, decimal OccupancyRate);
    public record RecentPaymentDto(Guid Id, string RenterName, string ApartmentNumber, decimal Amount, DateTime? PaymentDate, string Status);
    public record ActivityFeedItemDto(Guid Id, string ActorName, string Action, string Description, DateTime CreatedAt);

    public record OwnerDashboardDto(
        DashboardSummaryDto Summary, List<MonthlyRevenuePointDto> RevenueTrend, List<OccupancyPointDto> OccupancyTrend,
        List<RecentPaymentDto> RecentPayments, List<ActivityFeedItemDto> RecentActivity
    );
}

namespace ApartmentRental.Application.Dashboard.Interfaces
{
    using ApartmentRental.Application.Dashboard.DTOs;

    public interface IDashboardService
    {
        Task<OwnerDashboardDto> GetOwnerDashboardAsync(Guid ownerId, CancellationToken cancellationToken = default);
    }
}

namespace ApartmentRental.Application.Dashboard.Services
{
    using ApartmentRental.Application.Dashboard.DTOs;
    using ApartmentRental.Application.Dashboard.Interfaces;

    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<OwnerDashboardDto> GetOwnerDashboardAsync(Guid ownerId, CancellationToken cancellationToken = default)
        {
            var apartments = await _unitOfWork.Repository<Apartment>().Query()
                .Where(a => a.OwnerId == ownerId)
                .ToListAsync(cancellationToken);

            var apartmentIds = apartments.Select(a => a.Id).ToList();

            var payments = await _unitOfWork.Repository<Payment>().Query()
                .Include(p => p.Lease).ThenInclude(l => l.Apartment)
                .Include(p => p.Lease).ThenInclude(l => l.Renter)
                .Where(p => apartmentIds.Contains(p.Lease.ApartmentId))
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var monthlyRevenue = payments
                .Where(p => p.Status == PaymentStatus.Paid && p.PaymentDate >= startOfMonth)
                .Sum(p => p.Amount);

            var pending = payments.Where(p => p.Status == PaymentStatus.Pending).ToList();
            var late = payments.Where(p => p.Status == PaymentStatus.Pending && p.DueDate < now).ToList();

            var openMaintenance = await _unitOfWork.Repository<MaintenanceRequest>().Query()
                .Where(m => apartmentIds.Contains(m.ApartmentId) && m.Status != MaintenanceStatus.Completed && m.Status != MaintenanceStatus.Cancelled)
                .CountAsync(cancellationToken);

            var summary = new DashboardSummaryDto(
                apartments.Count,
                apartments.Count(a => a.Status == ApartmentStatus.Occupied),
                apartments.Count(a => a.Status == ApartmentStatus.Available),
                apartments.Count(a => a.Status == ApartmentStatus.Maintenance),
                monthlyRevenue, pending.Sum(p => p.Amount), pending.Count, late.Count, openMaintenance
            );

            var revenueTrend = Enumerable.Range(0, 6)
                .Select(offset => startOfMonth.AddMonths(-5 + offset))
                .Select(month => new MonthlyRevenuePointDto(
                    month.ToString("MMM yyyy"),
                    payments.Where(p => p.Status == PaymentStatus.Paid && p.PaymentDate.HasValue
                        && p.PaymentDate.Value.Year == month.Year && p.PaymentDate.Value.Month == month.Month)
                        .Sum(p => p.Amount)))
                .ToList();

            var totalApartments = Math.Max(apartments.Count, 1);
            var occupancyTrend = Enumerable.Range(0, 6)
                .Select(offset => startOfMonth.AddMonths(-5 + offset))
                .Select(month => new OccupancyPointDto(
                    month.ToString("MMM yyyy"),
                    Math.Round((decimal)apartments.Count(a => a.Status == ApartmentStatus.Occupied) / totalApartments * 100, 1)))
                .ToList();

            var recentPayments = payments
                .OrderByDescending(p => p.PaymentDate ?? p.CreatedAt)
                .Take(5)
                .Select(p => new RecentPaymentDto(p.Id, p.Lease.Renter.FullName, p.Lease.Apartment.ApartmentNumber, p.Amount, p.PaymentDate, p.Status.ToString()))
                .ToList();

            var recentActivity = await _unitOfWork.Repository<ActivityLog>().Query()
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new ActivityFeedItemDto(a.Id, a.ActorName, a.Action, a.Description, a.CreatedAt))
                .ToListAsync(cancellationToken);

            return new OwnerDashboardDto(summary, revenueTrend, occupancyTrend, recentPayments, recentActivity);
        }
    }
}
