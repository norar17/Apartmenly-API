namespace ApartmentRental.Application.Common.Interfaces;

// Abstracts the actual SMS gateway (Twilio, Semaphore, Console...). Business
// code depends on ISmsService only, never on a specific provider.
public interface ISmsProvider
{
    string ProviderName { get; }
    Task<SmsSendResult> SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default);
}

public record SmsSendResult(bool Success, string? ErrorMessage = null);
