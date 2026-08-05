namespace ApartmentRental.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toAddress, string subject, string htmlBody, Guid? relatedRenterId = null, CancellationToken cancellationToken = default);
}
