using System.Text;
using System.Text.Json;
using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApartmentRental.Infrastructure.Sms;

// Sends SMS through textbee.dev, which turns a linked Android phone into an
// SMS gateway - free (uses your own SIM/plan, no per-message API fees).
// Requires an ApiKey and a DeviceId from the textbee.dev dashboard after
// linking the Android app. See https://textbee.dev.
public class TextBeeSmsProvider : ISmsProvider
{
    private readonly HttpClient _httpClient;
    private readonly SmsSettings _settings;
    private readonly ILogger<TextBeeSmsProvider> _logger;

    public TextBeeSmsProvider(HttpClient httpClient, IOptions<SmsSettings> settings, ILogger<TextBeeSmsProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _httpClient.BaseAddress ??= new Uri("https://api.textbee.dev/api/v1/gateway/");
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
    }

    public string ProviderName => "TextBee";

    public async Task<SmsSendResult> SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                recipients = new[] { PhoneNumberHelper.ToE164(toPhoneNumber) },
                message
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync($"devices/{_settings.DeviceId}/send-sms", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new SmsSendResult(true);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("TextBee SMS failed with status {Status}: {Body}", response.StatusCode, body);
            return new SmsSendResult(false, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TextBee SMS provider threw an exception");
            return new SmsSendResult(false, ex.Message);
        }
    }
}
