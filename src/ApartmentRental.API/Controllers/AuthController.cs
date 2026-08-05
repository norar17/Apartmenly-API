using ApartmentRental.Application.Auth.DTOs;
using ApartmentRental.Application.Auth.Interfaces;
using ApartmentRental.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApartmentRental.API.Controllers;

public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register/owner")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<MagicLinkRequestedResponse>>> RegisterOwner(RegisterOwnerRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.RegisterOwnerAsync(request, cancellationToken));

    [HttpPost("register/renter")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<MagicLinkRequestedResponse>>> RegisterRenter(RegisterRenterRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.RegisterRenterAsync(request, cancellationToken));

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.LoginAsync(request, cancellationToken));

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.RefreshTokenAsync(request, cancellationToken));

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.ChangePasswordAsync(CurrentUserId, CurrentUserRole, request, cancellationToken));

    [HttpPost("magic-link/request")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<MagicLinkRequestedResponse>>> RequestMagicLink(RequestMagicLinkRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.RequestMagicLinkAsync(request, cancellationToken));

    [HttpPost("magic-link/verify")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> VerifyMagicLink(VerifyMagicLinkRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.VerifyMagicLinkAsync(request, cancellationToken));

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<MagicLinkRequestedResponse>>> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.ForgotPasswordAsync(request, cancellationToken));

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
        => FromResult(await _authService.ResetPasswordAsync(request, cancellationToken));
}
