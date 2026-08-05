namespace ApartmentRental.Application.Common.Interfaces;

public interface ISmsService
{
    Task SendAsync(string toPhoneNumber, string message, Guid? relatedRenterId = null, CancellationToken cancellationToken = default);
}
