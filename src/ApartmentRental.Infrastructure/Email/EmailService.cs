using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;

namespace ApartmentRental.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly IEmailProvider _provider;
    private readonly IUnitOfWork _unitOfWork;

    public EmailService(IEmailProvider provider, IUnitOfWork unitOfWork)
    {
        _provider = provider;
        _unitOfWork = unitOfWork;
    }

    public async Task SendAsync(string toAddress, string subject, string htmlBody, Guid? relatedRenterId = null, CancellationToken cancellationToken = default)
    {
        var result = await _provider.SendAsync(toAddress, subject, htmlBody, cancellationToken);

        var log = new EmailLog
        {
            ToAddress = toAddress,
            Subject = subject,
            Body = htmlBody,
            RelatedRenterId = relatedRenterId,
            Status = result.Success ? CommunicationStatus.Sent : CommunicationStatus.Failed,
            ErrorMessage = result.ErrorMessage,
            SentAt = result.Success ? DateTime.UtcNow : null
        };

        await _unitOfWork.Repository<EmailLog>().AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
