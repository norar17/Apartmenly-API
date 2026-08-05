using System.Reflection;
using ApartmentRental.Application.Apartments.Interfaces;
using ApartmentRental.Application.Apartments.Services;
using ApartmentRental.Application.Auth.Interfaces;
using ApartmentRental.Application.Auth.Services;
using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Application.Common.Services;
using ApartmentRental.Application.Dashboard.Interfaces;
using ApartmentRental.Application.Dashboard.Services;
using ApartmentRental.Application.Leases.Interfaces;
using ApartmentRental.Application.Leases.Services;
using ApartmentRental.Application.Maintenance.Interfaces;
using ApartmentRental.Application.Maintenance.Services;
using ApartmentRental.Application.Notifications.Interfaces;
using ApartmentRental.Application.Notifications.Services;
using ApartmentRental.Application.Payments.Interfaces;
using ApartmentRental.Application.Payments.Services;
using ApartmentRental.Application.Renters.Interfaces;
using ApartmentRental.Application.Renters.Services;
using ApartmentRental.Application.Reports.Interfaces;
using ApartmentRental.Application.Reports.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ApartmentRental.Application.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IActivityLogger, ActivityLogger>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IApartmentService, ApartmentService>();
        services.AddScoped<IRenterService, RenterService>();
        services.AddScoped<ILeaseService, LeaseService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();

        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationQueryService, NotificationQueryService>();

        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
