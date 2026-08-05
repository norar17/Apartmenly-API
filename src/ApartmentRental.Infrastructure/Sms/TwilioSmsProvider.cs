using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApartmentRental.Infrastructure.Sms;

public class TwilioSmsProvider : ISmsProvider
{
    private readonly HttpClient _httpClient;
    private readonly SmsSettings _settings;
    private readonly ILogger<TwilioSmsProvider> _logger;

    public TwilioSmsProvider(HttpClient httpClient, IOptions<SmsSettings> settings, ILogger<TwilioSmsProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public string ProviderName => "Twilio";

    public async Task<SmsSendResult> SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"https://api.twilio.com/2010-04-01/Accounts/{_settings.AccountSid}/Messages.json";
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_settings.AccountSid}:{_settings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var form = new Dictionary<string, string>
            {
                ["To"] = PhoneNumberHelper.ToE164(toPhoneNumber),
                ["From"] = _settings.SenderId,
                ["Body"] = message
            };

            using var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(form), cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return new SmsSendResult(true);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Twilio SMS failed with status {Status}: {Body}", response.StatusCode, body);
            return new SmsSendResult(false, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio SMS provider threw an exception");
            return new SmsSendResult(false, ex.Message);
        }
    }
}
