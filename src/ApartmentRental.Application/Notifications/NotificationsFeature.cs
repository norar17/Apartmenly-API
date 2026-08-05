using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Application.Common.Models;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;
using ApartmentRental.Shared;
using Microsoft.EntityFrameworkCore;

namespace ApartmentRental.Application.Notifications.DTOs
{
    public record NotificationDto(Guid Id, string Title, string Message, NotificationType Type, bool IsRead, string? LinkUrl, DateTime CreatedAt);
}

namespace ApartmentRental.Application.Notifications.Interfaces
{
    using ApartmentRental.Application.Notifications.DTOs;

    public interface INotificationQueryService
    {
        Task<PagedResult<NotificationDto>> GetForUserAsync(Guid userId, UserRole role, PaginationParams pagination, CancellationToken cancellationToken = default);
        Task<int> GetUnreadCountAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default);
        Task<Result> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
        Task<Result> MarkAllAsReadAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default);
    }
}

namespace ApartmentRental.Application.Notifications.Services
{
    using ApartmentRental.Application.Notifications.DTOs;
    using ApartmentRental.Application.Notifications.Interfaces;

    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task NotifyAsync(Guid userId, UserRole userRole, string title, string message,
            NotificationType type = NotificationType.General, string? linkUrl = null,
            CancellationToken cancellationToken = default)
        {
            var notification = new Notification
            {
                UserId = userId,
                UserRole = userRole,
                Title = title,
                Message = message,
                Type = type,
                LinkUrl = linkUrl
            };

            await _unitOfWork.Repository<Notification>().AddAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public class NotificationQueryService : INotificationQueryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationQueryService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<PagedResult<NotificationDto>> GetForUserAsync(Guid userId, UserRole role, PaginationParams pagination, CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.Repository<Notification>().Query()
                .Where(n => n.UserId == userId && n.UserRole == role)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            var items = entities.Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.Type, n.IsRead, n.LinkUrl, n.CreatedAt));

            return PagedResult<NotificationDto>.Create(items, totalCount, pagination.PageNumber, pagination.PageSize);
        }

        public Task<int> GetUnreadCountAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default)
        {
            return _unitOfWork.Repository<Notification>().Query()
                .Where(n => n.UserId == userId && n.UserRole == role && !n.IsRead)
                .CountAsync(cancellationToken);
        }

        public async Task<Result> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            var notifications = _unitOfWork.Repository<Notification>();
            var notification = await notifications.Query(asNoTracking: false)
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

            if (notification is null)
            {
                return Result.NotFound("Notification", notificationId);
            }

            notification.IsRead = true;
            notifications.Update(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result> MarkAllAsReadAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default)
        {
            var notifications = _unitOfWork.Repository<Notification>();
            var unread = await notifications.Query(asNoTracking: false)
                .Where(n => n.UserId == userId && n.UserRole == role && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var notification in unread)
            {
                notification.IsRead = true;
                notifications.Update(notification);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
