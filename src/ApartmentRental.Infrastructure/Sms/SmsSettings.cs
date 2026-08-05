namespace ApartmentRental.Infrastructure.Sms;

public class SmsSettings
{
    public const string SectionName = "Sms";

    // SMS is currently disconnected from the app - the reminder flow only
    // sends email now, and there's no SMS test endpoint. This whole
    // ISmsService/ISmsProvider stack (Twilio, Semaphore, TextBee, Console)
    // still works and is ready to wire back in later; it's just unused for
    // now. Console is the safe default: a no-op that only logs.
    public string Provider { get; set; } = "Console";

    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string AccountSid { get; set; } = string.Empty; // Twilio-specific
    public string DeviceId { get; set; } = string.Empty; // TextBee-specific
}
