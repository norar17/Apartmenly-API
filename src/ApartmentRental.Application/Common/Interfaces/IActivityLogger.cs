namespace ApartmentRental.Application.Common.Interfaces;

public interface IActivityLogger
{
    Task LogAsync(string action, string description, string? entityType = null, Guid? entityId = null, CancellationToken cancellationToken = default);
}
