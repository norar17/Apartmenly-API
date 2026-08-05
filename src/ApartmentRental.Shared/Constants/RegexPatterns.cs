namespace ApartmentRental.Shared.Constants;

public static class RegexPatterns
{
    public const string PhMobileNumber = @"^(09\d{9}|\+63\d{10})$";
    public const string PhMobileNumberMessage = "Enter a valid PH mobile number (09XXXXXXXXX or +63XXXXXXXXXX).";
}
