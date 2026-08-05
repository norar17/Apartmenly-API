namespace ApartmentRental.Shared;

// PH numbers are entered/stored as either 09XXXXXXXXX or +63XXXXXXXXXX (see
// Constants/RegexPatterns.cs). Most SMS gateways (Twilio in particular)
// require full E.164 format, so this is the one place every provider
// normalizes to before sending.
public static class PhoneNumberHelper
{
    public static string ToE164(string phoneNumber)
    {
        var trimmed = phoneNumber.Trim();
        return trimmed.StartsWith("09")
            ? "+63" + trimmed[1..]
            : trimmed;
    }
}
