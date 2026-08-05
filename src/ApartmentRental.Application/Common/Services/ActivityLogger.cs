using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Interfaces;

namespace ApartmentRental.Application.Common.Services;

public class ActivityLogger : IActivityLogger
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ActivityLogger(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task LogAsync(string action, string description, string? entityType = null, Guid? entityId = null, CancellationToken cancellationToken = default)
    {
        var log = new ActivityLog
        {
            ActorId = _currentUser.UserId,
            ActorName = _currentUser.Email ?? "System",
            Action = action,
            Description = description,
            EntityType = entityType,
            EntityId = entityId
        };

        await _unitOfWork.Repository<ActivityLog>().AddAsync(log, cancellationToken);
    }
}
