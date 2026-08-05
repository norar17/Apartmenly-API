using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;

namespace ApartmentRental.Infrastructure.Sms;

public class SmsService : ISmsService
{
    private readonly ISmsProvider _provider;
    private readonly IUnitOfWork _unitOfWork;

    public SmsService(ISmsProvider provider, IUnitOfWork unitOfWork)
    {
        _provider = provider;
        _unitOfWork = unitOfWork;
    }

    public async Task SendAsync(string toPhoneNumber, string message, Guid? relatedRenterId = null, CancellationToken cancellationToken = default)
    {
        var result = await _provider.SendAsync(toPhoneNumber, message, cancellationToken);

        var log = new SmsLog
        {
            ToPhoneNumber = toPhoneNumber,
            Message = message,
            Provider = _provider.ProviderName,
            RelatedRenterId = relatedRenterId,
            Status = result.Success ? CommunicationStatus.Sent : CommunicationStatus.Failed,
            ErrorMessage = result.ErrorMessage,
            SentAt = result.Success ? DateTime.UtcNow : null
        };

        await _unitOfWork.Repository<SmsLog>().AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
