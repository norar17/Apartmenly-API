using ApartmentRental.Domain.Enums;

namespace ApartmentRental.Application.Common.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(Guid userId, UserRole userRole, string title, string message,
        NotificationType type = NotificationType.General, string? linkUrl = null,
        CancellationToken cancellationToken = default);
}
