using ApartmentRental.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace ApartmentRental.Infrastructure.Sms;

public class ConsoleSmsProvider : ISmsProvider
{
    private readonly ILogger<ConsoleSmsProvider> _logger;

    public ConsoleSmsProvider(ILogger<ConsoleSmsProvider> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "Console";

    public Task<SmsSendResult> SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[DEV SMS] To: {To} | Message: {Message}", toPhoneNumber, message);
        return Task.FromResult(new SmsSendResult(true));
    }
}
