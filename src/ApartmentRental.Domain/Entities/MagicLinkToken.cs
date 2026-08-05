using ApartmentRental.Domain.Common;
using ApartmentRental.Domain.Enums;

namespace ApartmentRental.Domain.Entities;

public class MagicLinkToken : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Token { get; set; } = string.Empty;
    public MagicLinkPurpose Purpose { get; set; } = MagicLinkPurpose.SignIn;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? ConsumedAtUtc { get; set; }
}
