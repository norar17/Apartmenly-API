using ApartmentRental.Domain.Enums;

namespace ApartmentRental.Application.Auth.DTOs;

public record RegisterOwnerRequest(string FullName, string Email, string PhoneNumber, string Password, string? CompanyName);

public record RegisterRenterRequest(string FullName, string Email, string PhoneNumber, string Password, string? Address);

public record LoginRequest(string Email, string Password, UserRole Role);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserProfileDto User);

public record UserProfileDto(Guid Id, string FullName, string Email, string PhoneNumber, UserRole Role, string? AvatarUrl, bool EmailVerified);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record RequestMagicLinkRequest(string Email, UserRole Role);

public record VerifyMagicLinkRequest(string Token);

public record MagicLinkRequestedResponse(string Message);

public record ForgotPasswordRequest(string Email, UserRole Role);

public record ResetPasswordRequest(string Token, string NewPassword);
