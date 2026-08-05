using ApartmentRental.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApartmentRental.Infrastructure.Sms;

public class SemaphoreSmsProvider : ISmsProvider
{
    private readonly HttpClient _httpClient;
    private readonly SmsSettings _settings;
    private readonly ILogger<SemaphoreSmsProvider> _logger;

    public SemaphoreSmsProvider(HttpClient httpClient, IOptions<SmsSettings> settings, ILogger<SemaphoreSmsProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public string ProviderName => "Semaphore";

    public async Task<SmsSendResult> SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var form = new Dictionary<string, string>
            {
                ["apikey"] = _settings.ApiKey,
                ["number"] = toPhoneNumber,
                ["message"] = message,
                ["sendername"] = _settings.SenderId
            };

            using var response = await _httpClient.PostAsync(
                "https://api.semaphore.co/api/v4/messages", new FormUrlEncodedContent(form), cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new SmsSendResult(true);
            }

            _logger.LogWarning("Semaphore SMS failed with status {Status}: {Body}", response.StatusCode, body);
            return new SmsSendResult(false, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Semaphore SMS provider threw an exception");
            return new SmsSendResult(false, ex.Message);
        }
    }
}
