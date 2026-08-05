using ApartmentRental.Application.Auth.DTOs;
using ApartmentRental.Application.Auth.Interfaces;
using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;
using ApartmentRental.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ApartmentRental.Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IDateTimeService _dateTime;
    private readonly IActivityLogger _activityLogger;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    private const int AccessTokenLifetimeMinutes = 60;
    private const int RefreshTokenLifetimeDays = 14;
    private const int MagicLinkLifetimeMinutes = 15;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IDateTimeService dateTime,
        IActivityLogger activityLogger,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _dateTime = dateTime;
        _activityLogger = activityLogger;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<Result<MagicLinkRequestedResponse>> RegisterOwnerAsync(RegisterOwnerRequest request, CancellationToken cancellationToken = default)
    {
        var owners = _unitOfWork.Repository<Owner>();
        var emailTaken = await owners.ExistsAsync(o => o.Email == request.Email, cancellationToken);
        if (emailTaken)
        {
            return Result.Failure<MagicLinkRequestedResponse>("An account with this email already exists.", "EMAIL_TAKEN");
        }

        var owner = new Owner
        {
            FullName = request.FullName,
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CompanyName = request.CompanyName
        };

        await owners.AddAsync(owner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync("Owner.Registered", $"{owner.FullName} created an owner account.", nameof(Owner), owner.Id, cancellationToken);

        await SendMagicLinkAsync(owner.Email, owner.FullName, UserRole.Owner, MagicLinkPurpose.SignIn, isWelcome: true, cancellationToken);

        return Result.Success(new MagicLinkRequestedResponse("Account created. Check your email to verify and sign in."));
    }

    public async Task<Result<MagicLinkRequestedResponse>> RegisterRenterAsync(RegisterRenterRequest request, CancellationToken cancellationToken = default)
    {
        var renters = _unitOfWork.Repository<Renter>();
        var emailTaken = await renters.ExistsAsync(r => r.Email == request.Email, cancellationToken);
        if (emailTaken)
        {
            return Result.Failure<MagicLinkRequestedResponse>("An account with this email already exists.", "EMAIL_TAKEN");
        }

        var renter = new Renter
        {
            FullName = request.FullName,
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Address = request.Address
        };

        await renters.AddAsync(renter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync("Renter.Registered", $"{renter.FullName} created a renter account.", nameof(Renter), renter.Id, cancellationToken);

        await SendMagicLinkAsync(renter.Email, renter.FullName, UserRole.Renter, MagicLinkPurpose.SignIn, isWelcome: true, cancellationToken);

        return Result.Success(new MagicLinkRequestedResponse("Account created. Check your email to verify and sign in."));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (request.Role == UserRole.Owner)
        {
            var owners = _unitOfWork.Repository<Owner>();
            var owner = await owners.Query(asNoTracking: false).FirstOrDefaultAsync(o => o.Email == email, cancellationToken);
            if (owner is null || !owner.IsActive || !_passwordHasher.Verify(request.Password, owner.PasswordHash))
            {
                return Result.Failure<AuthResponse>("Invalid email or password.", "INVALID_CREDENTIALS");
            }

            IssueRefreshToken(owner);
            owners.Update(owner);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(BuildAuthResponse(owner.Id, owner.Email, owner.FullName, owner.PhoneNumber, UserRole.Owner, owner.AvatarUrl, owner.EmailVerifiedAt is not null, owner.RefreshToken!));
        }
        else
        {
            var renters = _unitOfWork.Repository<Renter>();
            var renter = await renters.Query(asNoTracking: false).FirstOrDefaultAsync(r => r.Email == email, cancellationToken);
            if (renter is null || !renter.IsActive || !_passwordHasher.Verify(request.Password, renter.PasswordHash))
            {
                return Result.Failure<AuthResponse>("Invalid email or password.", "INVALID_CREDENTIALS");
            }

            IssueRefreshToken(renter);
            renters.Update(renter);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(BuildAuthResponse(renter.Id, renter.Email, renter.FullName, renter.PhoneNumber, UserRole.Renter, renter.AvatarUrl, renter.EmailVerifiedAt is not null, renter.RefreshToken!));
        }
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var owners = _unitOfWork.Repository<Owner>();
        var owner = await owners.Query(asNoTracking: false).FirstOrDefaultAsync(o => o.RefreshToken == request.RefreshToken, cancellationToken);
        if (owner is not null)
        {
            if (owner.RefreshTokenExpiresAt is null || owner.RefreshTokenExpiresAt < _dateTime.UtcNow)
            {
                return Result.Failure<AuthResponse>("Refresh token expired. Please log in again.", "TOKEN_EXPIRED");
            }

            IssueRefreshToken(owner);
            owners.Update(owner);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(BuildAuthResponse(owner.Id, owner.Email, owner.FullName, owner.PhoneNumber, UserRole.Owner, owner.AvatarUrl, owner.EmailVerifiedAt is not null, owner.RefreshToken!));
        }

        var renters = _unitOfWork.Repository<Renter>();
        var renter = await renters.Query(asNoTracking: false).FirstOrDefaultAsync(r => r.RefreshToken == request.RefreshToken, cancellationToken);
        if (renter is not null)
        {
            if (renter.RefreshTokenExpiresAt is null || renter.RefreshTokenExpiresAt < _dateTime.UtcNow)
            {
                return Result.Failure<AuthResponse>("Refresh token expired. Please log in again.", "TOKEN_EXPIRED");
            }

            IssueRefreshToken(renter);
            renters.Update(renter);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(BuildAuthResponse(renter.Id, renter.Email, renter.FullName, renter.PhoneNumber, UserRole.Renter, renter.AvatarUrl, renter.EmailVerifiedAt is not null, renter.RefreshToken!));
        }

        return Result.Failure<AuthResponse>("Invalid refresh token.", "INVALID_TOKEN");
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, UserRole role, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (role == UserRole.Owner)
        {
            var owners = _unitOfWork.Repository<Owner>();
            var owner = await owners.Query(asNoTracking: false).FirstOrDefaultAsync(o => o.Id == userId, cancellationToken);
            if (owner is null) return Result.Failure("Account not found.", "NOT_FOUND");
            if (!_passwordHasher.Verify(request.CurrentPassword, owner.PasswordHash)) return Result.Failure("Current password is incorrect.", "INVALID_CREDENTIALS");

            owner.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            owners.Update(owner);
        }
        else
        {
            var renters = _unitOfWork.Repository<Renter>();
            var renter = await renters.Query(asNoTracking: false).FirstOrDefaultAsync(r => r.Id == userId, cancellationToken);
            if (renter is null) return Result.Failure("Account not found.", "NOT_FOUND");
            if (!_passwordHasher.Verify(request.CurrentPassword, renter.PasswordHash)) return Result.Failure("Current password is incorrect.", "INVALID_CREDENTIALS");

            renter.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            renters.Update(renter);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<MagicLinkRequestedResponse>> RequestMagicLinkAsync(RequestMagicLinkRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        string fullName;

        if (request.Role == UserRole.Owner)
        {
            var owner = await _unitOfWork.Repository<Owner>().FindAsync(o => o.Email == email, cancellationToken);
            if (owner is null || !owner.IsActive)
            {
                return Result.Failure<MagicLinkRequestedResponse>("No owner account found with that email.", "NOT_FOUND");
            }
            fullName = owner.FullName;
        }
        else
        {
            var renter = await _unitOfWork.Repository<Renter>().FindAsync(r => r.Email == email, cancellationToken);
            if (renter is null || !renter.IsActive)
            {
                return Result.Failure<MagicLinkRequestedResponse>("No renter account found with that email.", "NOT_FOUND");
            }
            fullName = renter.FullName;
        }

        await SendMagicLinkAsync(email, fullName, request.Role, MagicLinkPurpose.SignIn, isWelcome: false, cancellationToken);

        return Result.Success(new MagicLinkRequestedResponse("A sign-in link has been sent to your email."));
    }

    // Single-use, time-limited: the token is marked consumed immediately so
    // a second click on the same link fails even within the expiry window.
    // Only matches SignIn-purpose tokens - a password-reset link can't be
    // used to sign in without also going through ResetPasswordAsync.
    public async Task<Result<AuthResponse>> VerifyMagicLinkAsync(VerifyMagicLinkRequest request, CancellationToken cancellationToken = default)
    {
        var tokens = _unitOfWork.Repository<MagicLinkToken>();
        var magicLink = await tokens.Query(asNoTracking: false)
            .FirstOrDefaultAsync(m => m.Token == request.Token && m.Purpose == MagicLinkPurpose.SignIn, cancellationToken);

        if (magicLink is null || magicLink.ConsumedAtUtc is not null || magicLink.ExpiresAtUtc < _dateTime.UtcNow)
        {
            return Result.Failure<AuthResponse>("This sign-in link is invalid or has expired.", "INVALID_TOKEN");
        }

        magicLink.ConsumedAtUtc = _dateTime.UtcNow;
        tokens.Update(magicLink);

        if (magicLink.Role == UserRole.Owner)
        {
            var owner = await _unitOfWork.Repository<Owner>().Query(asNoTracking: false).FirstOrDefaultAsync(o => o.Email == magicLink.Email, cancellationToken);
            if (owner is null)
            {
                return Result.Failure<AuthResponse>("Account not found.", "NOT_FOUND");
            }

            owner.EmailVerifiedAt ??= _dateTime.UtcNow;
            IssueRefreshToken(owner);
            _unitOfWork.Repository<Owner>().Update(owner);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _activityLogger.LogAsync("Owner.MagicLinkLogin", $"{owner.FullName} signed in via magic link.", nameof(Owner), owner.Id, cancellationToken);

            return Result.Success(BuildAuthResponse(owner.Id, owner.Email, owner.FullName, owner.PhoneNumber, UserRole.Owner, owner.AvatarUrl, true, owner.RefreshToken!));
        }
        else
        {
            var renter = await _unitOfWork.Repository<Renter>().Query(asNoTracking: false).FirstOrDefaultAsync(r => r.Email == magicLink.Email, cancellationToken);
            if (renter is null)
            {
                return Result.Failure<AuthResponse>("Account not found.", "NOT_FOUND");
            }

            renter.EmailVerifiedAt ??= _dateTime.UtcNow;
            IssueRefreshToken(renter);
            _unitOfWork.Repository<Renter>().Update(renter);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _activityLogger.LogAsync("Renter.MagicLinkLogin", $"{renter.FullName} signed in via magic link.", nameof(Renter), renter.Id, cancellationToken);

            return Result.Success(BuildAuthResponse(renter.Id, renter.Email, renter.FullName, renter.PhoneNumber, UserRole.Renter, renter.AvatarUrl, true, renter.RefreshToken!));
        }
    }

    public async Task<Result<MagicLinkRequestedResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        string fullName;

        if (request.Role == UserRole.Owner)
        {
            var owner = await _unitOfWork.Repository<Owner>().FindAsync(o => o.Email == email, cancellationToken);
            if (owner is null || !owner.IsActive)
            {
                return Result.Failure<MagicLinkRequestedResponse>("No owner account found with that email.", "NOT_FOUND");
            }
            fullName = owner.FullName;
        }
        else
        {
            var renter = await _unitOfWork.Repository<Renter>().FindAsync(r => r.Email == email, cancellationToken);
            if (renter is null || !renter.IsActive)
            {
                return Result.Failure<MagicLinkRequestedResponse>("No renter account found with that email.", "NOT_FOUND");
            }
            fullName = renter.FullName;
        }

        await SendMagicLinkAsync(email, fullName, request.Role, MagicLinkPurpose.PasswordReset, isWelcome: false, cancellationToken);

        return Result.Success(new MagicLinkRequestedResponse("A password reset link has been sent to your email."));
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var tokens = _unitOfWork.Repository<MagicLinkToken>();
        var resetToken = await tokens.Query(asNoTracking: false)
            .FirstOrDefaultAsync(m => m.Token == request.Token && m.Purpose == MagicLinkPurpose.PasswordReset, cancellationToken);

        if (resetToken is null || resetToken.ConsumedAtUtc is not null || resetToken.ExpiresAtUtc < _dateTime.UtcNow)
        {
            return Result.Failure("This password reset link is invalid or has expired.", "INVALID_TOKEN");
        }

        resetToken.ConsumedAtUtc = _dateTime.UtcNow;
        tokens.Update(resetToken);

        if (resetToken.Role == UserRole.Owner)
        {
            var owners = _unitOfWork.Repository<Owner>();
            var owner = await owners.Query(asNoTracking: false).FirstOrDefaultAsync(o => o.Email == resetToken.Email, cancellationToken);
            if (owner is null) return Result.Failure("Account not found.", "NOT_FOUND");

            owner.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            owners.Update(owner);
        }
        else
        {
            var renters = _unitOfWork.Repository<Renter>();
            var renter = await renters.Query(asNoTracking: false).FirstOrDefaultAsync(r => r.Email == resetToken.Email, cancellationToken);
            if (renter is null) return Result.Failure("Account not found.", "NOT_FOUND");

            renter.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            renters.Update(renter);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _activityLogger.LogAsync($"{resetToken.Role}.PasswordReset", $"{resetToken.Email} reset their password.", null, null, cancellationToken);

        return Result.Success();
    }

    // Shared by registration (welcome + verify), the login "email link" flow,
    // and forgot-password - only the email copy and token purpose differ.
    private async Task SendMagicLinkAsync(string email, string fullName, UserRole role, MagicLinkPurpose purpose, bool isWelcome, CancellationToken cancellationToken)
    {
        var token = _tokenGenerator.GenerateRefreshToken();

        var magicLink = new MagicLinkToken
        {
            Email = email,
            Role = role,
            Token = token,
            Purpose = purpose,
            ExpiresAtUtc = _dateTime.UtcNow.AddMinutes(MagicLinkLifetimeMinutes)
        };

        await _unitOfWork.Repository<MagicLinkToken>().AddAsync(magicLink, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";

        string subject, heading, intro, buttonText, link;

        if (purpose == MagicLinkPurpose.PasswordReset)
        {
            link = $"{frontendBaseUrl}/reset-password?token={Uri.EscapeDataString(token)}";
            subject = "Reset your Apartly password";
            heading = "Reset your password";
            intro = "We got a request to reset your password. Click below to choose a new one.";
            buttonText = "Reset password";
        }
        else
        {
            link = $"{frontendBaseUrl}/magic-link/verify?token={Uri.EscapeDataString(token)}";
            subject = isWelcome ? "Welcome to Apartly — verify your email" : "Your Apartly sign-in link";
            heading = isWelcome ? $"Hi {fullName}, welcome to Apartly" : "Sign in to Apartly";
            intro = isWelcome
                ? "Thanks for creating an account. Click below to verify your email and sign in."
                : "Click below to sign in. This link is single-use.";
            buttonText = isWelcome ? "Verify & sign in" : "Sign in";
        }

        var body = EmailTemplate.Build(
            heading,
            $"<p style=\"margin:0 0 4px;\">{intro}</p><p style=\"margin:12px 0 0;font-size:13px;color:#9C9DAE;\">This link expires in {MagicLinkLifetimeMinutes} minutes.</p>",
            buttonText,
            link);

        await _emailService.SendAsync(email, subject, body, null, cancellationToken);
    }

    private void IssueRefreshToken(Owner owner)
    {
        owner.RefreshToken = _tokenGenerator.GenerateRefreshToken();
        owner.RefreshTokenExpiresAt = _dateTime.UtcNow.AddDays(RefreshTokenLifetimeDays);
    }

    private void IssueRefreshToken(Renter renter)
    {
        renter.RefreshToken = _tokenGenerator.GenerateRefreshToken();
        renter.RefreshTokenExpiresAt = _dateTime.UtcNow.AddDays(RefreshTokenLifetimeDays);
    }

    private AuthResponse BuildAuthResponse(Guid id, string email, string fullName, string phone, UserRole role, string? avatarUrl, bool emailVerified, string refreshToken)
    {
        var accessToken = _tokenGenerator.GenerateAccessToken(id, email, fullName, role);
        var expiresAt = _dateTime.UtcNow.AddMinutes(AccessTokenLifetimeMinutes);
        var profile = new UserProfileDto(id, fullName, email, phone, role, avatarUrl, emailVerified);
        return new AuthResponse(accessToken, refreshToken, expiresAt, profile);
    }
}
