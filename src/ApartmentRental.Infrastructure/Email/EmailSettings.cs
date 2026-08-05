namespace ApartmentRental.Infrastructure.Email;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = "Resend";

    public string SenderName { get; set; } = "Apartment Management";
    public string SenderEmail { get; set; } = string.Empty;

    public string ResendApiKey { get; set; } = string.Empty;

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool SmtpUseSsl { get; set; } = true;
}
