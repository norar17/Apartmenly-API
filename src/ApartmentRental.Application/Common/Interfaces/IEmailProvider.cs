namespace ApartmentRental.Application.Common.Interfaces;

// Abstracts the actual email gateway (Resend, SMTP...). Business code
// depends on IEmailService only, never on a specific provider.
public interface IEmailProvider
{
    string ProviderName { get; }
    Task<EmailSendResult> SendAsync(string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

public record EmailSendResult(bool Success, string? ErrorMessage = null);
