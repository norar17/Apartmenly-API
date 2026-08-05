using System.Text;
using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Domain.Interfaces;
using ApartmentRental.Infrastructure.Auth;
using ApartmentRental.Infrastructure.BackgroundServices;
using ApartmentRental.Infrastructure.Email;
using ApartmentRental.Infrastructure.Persistence;
using ApartmentRental.Infrastructure.Sms;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ApartmentRental.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            // EF Core's pending-model-changes check is stricter than needed here
            // and can false-trigger between machines/SDK patch versions even with
            // no real model drift. Migrations still run normally; this only stops
            // that specific warning from being treated as fatal.
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<SmsSettings>(configuration.GetSection(SmsSettings.SectionName));

        services.AddHttpContextAccessor();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeService, SystemDateTimeService>();

        RegisterEmailProvider(services, configuration);
        services.AddScoped<IEmailService, EmailService>();

        RegisterSmsProvider(services, configuration);
        services.AddScoped<ISmsService, SmsService>();

        services.AddHostedService<PaymentReminderBackgroundService>();

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy(Shared.Constants.Policies.OwnerOnly, policy => policy.RequireRole(Shared.Constants.Roles.Owner))
            .AddPolicy(Shared.Constants.Policies.RenterOnly, policy => policy.RequireRole(Shared.Constants.Roles.Renter));

        return services;
    }

    // Reads Email:Provider from config and swaps the implementation - the
    // only place that needs to change to add a new email gateway.
    private static void RegisterEmailProvider(IServiceCollection services, IConfiguration configuration)
    {
        var providerName = configuration[$"{EmailSettings.SectionName}:Provider"] ?? "Resend";

        switch (providerName.Trim().ToLowerInvariant())
        {
            case "smtp":
                services.AddScoped<IEmailProvider, SmtpEmailProvider>();
                break;
            default:
                services.AddHttpClient<IEmailProvider, ResendEmailProvider>();
                break;
        }
    }

    // Same pattern for SMS - swap Sms:Provider in config, nothing else changes.
    private static void RegisterSmsProvider(IServiceCollection services, IConfiguration configuration)
    {
        var providerName = configuration[$"{SmsSettings.SectionName}:Provider"] ?? "Console";

        switch (providerName.Trim().ToLowerInvariant())
        {
            case "textbee":
                services.AddHttpClient<ISmsProvider, TextBeeSmsProvider>();
                break;
            case "twilio":
                services.AddHttpClient<ISmsProvider, TwilioSmsProvider>();
                break;
            case "semaphore":
                services.AddHttpClient<ISmsProvider, SemaphoreSmsProvider>();
                break;
            default:
                services.AddScoped<ISmsProvider, ConsoleSmsProvider>();
                break;
        }
    }
}

internal class SystemDateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateOnly TodayUtc => DateOnly.FromDateTime(DateTime.UtcNow);
}
