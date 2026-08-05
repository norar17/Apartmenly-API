using ApartmentRental.Application.Auth.DTOs;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Shared;

namespace ApartmentRental.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<Result<MagicLinkRequestedResponse>> RegisterOwnerAsync(RegisterOwnerRequest request, CancellationToken cancellationToken = default);
    Task<Result<MagicLinkRequestedResponse>> RegisterRenterAsync(RegisterRenterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(Guid userId, UserRole role, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result<MagicLinkRequestedResponse>> RequestMagicLinkAsync(RequestMagicLinkRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> VerifyMagicLinkAsync(VerifyMagicLinkRequest request, CancellationToken cancellationToken = default);
    Task<Result<MagicLinkRequestedResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
