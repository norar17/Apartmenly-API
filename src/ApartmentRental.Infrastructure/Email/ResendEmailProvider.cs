using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ApartmentRental.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApartmentRental.Infrastructure.Email;

public class ResendEmailProvider : IEmailProvider
{
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _settings;
    private readonly ILogger<ResendEmailProvider> _logger;

    public ResendEmailProvider(HttpClient httpClient, IOptions<EmailSettings> settings, ILogger<ResendEmailProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _httpClient.BaseAddress ??= new Uri("https://api.resend.com/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ResendApiKey);
    }

    public string ProviderName => "Resend";

    public async Task<EmailSendResult> SendAsync(string toAddress, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                from = $"{_settings.SenderName} <{_settings.SenderEmail}>",
                to = new[] { toAddress },
                subject,
                html = htmlBody
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync("emails", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new EmailSendResult(true);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Resend email failed with status {Status}: {Body}", response.StatusCode, body);
            return new EmailSendResult(false, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Resend email provider threw an exception");
            return new EmailSendResult(false, ex.Message);
        }
    }
}
