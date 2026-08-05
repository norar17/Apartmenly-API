using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using ApartmentRental.Domain.Interfaces;
using ApartmentRental.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ApartmentRental.Infrastructure.BackgroundServices;

// Runs daily; reminds renters at -3, 0, and +3 days from due date, then
// flags overdue pending payments as Late. Email + in-app notification only -
// SMS was pulled out for now (see ISmsService/SmsService if you want it back).
public class PaymentReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentReminderBackgroundService> _logger;
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);

    public PaymentReminderBackgroundService(IServiceProvider serviceProvider, ILogger<PaymentReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunReminderSweepAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment reminder sweep failed");
            }

            await Task.Delay(RunInterval, stoppingToken);
        }
    }

    private async Task RunReminderSweepAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var payments = unitOfWork.Repository<Payment>();
        var today = DateTime.UtcNow.Date;

        var pendingPayments = await payments.Query(asNoTracking: false)
            .Include(p => p.Lease).ThenInclude(l => l.Renter)
            .Include(p => p.Lease).ThenInclude(l => l.Apartment).ThenInclude(a => a.Owner)
            .Where(p => p.Status == PaymentStatus.Pending)
            .ToListAsync(cancellationToken);

        foreach (var payment in pendingPayments)
        {
            var daysUntilDue = (payment.DueDate.Date - today).Days;
            var renter = payment.Lease.Renter;
            var apartment = payment.Lease.Apartment;
            var apartmentNumber = apartment.ApartmentNumber;

            // 3 days before, on the due date, and 3 days after (then it becomes Late).
            var shouldRemind = daysUntilDue is 3 or 0 or -3;

            if (daysUntilDue < 0 && payment.Status == PaymentStatus.Pending)
            {
                payment.Status = PaymentStatus.Late;
                payments.Update(payment);

                // Fires exactly once, right as the payment flips to Late - lets
                // the owner know without a Pending payment ever getting silently
                // ignored.
                var owner = apartment.Owner;
                var ownerBody = BuildOwnerLateNoticeEmail(owner.FullName, renter.FullName, apartmentNumber, payment.Amount, payment.DueDate);
                await emailService.SendAsync(owner.Email, "Renter payment overdue", ownerBody, null, cancellationToken);
                await notificationService.NotifyAsync(owner.Id, UserRole.Owner, "Payment overdue",
                    $"{renter.FullName} hasn't paid for apartment {apartmentNumber} yet.", NotificationType.General, cancellationToken: cancellationToken);
            }

            if (!shouldRemind) continue;

            var subject = "Apartment Rental Payment Reminder";
            var body = BuildEmailBody(renter.FullName, apartmentNumber, payment.Amount, payment.DueDate, daysUntilDue);
            await emailService.SendAsync(renter.Email, subject, body, renter.Id, cancellationToken);

            var reminderMessage = BuildReminderMessage(apartmentNumber, payment.Amount, payment.DueDate, daysUntilDue);
            await notificationService.NotifyAsync(renter.Id, UserRole.Renter, "Payment reminder",
                reminderMessage, NotificationType.PaymentReminder, cancellationToken: cancellationToken);

            _logger.LogInformation("Sent payment reminder to {RenterEmail} for apartment {Apartment} ({DaysUntilDue} days from due date)",
                renter.Email, apartmentNumber, daysUntilDue);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string BuildOwnerLateNoticeEmail(string ownerName, string renterName, string apartmentNumber, decimal amount, DateTime dueDate)
    {
        var body = $@"
            <p style=""margin:0 0 4px;"">Hi {ownerName},</p>
            <p style=""margin:12px 0 0;""><strong style=""color:#14151F;"">{renterName}</strong> has not paid rent for
            apartment {apartmentNumber} — <strong style=""color:#14151F;"">&#8369;{amount:N2}</strong> was due on {dueDate:MMMM d}
            and is now overdue.</p>
            <p style=""margin:12px 0 0;"">A reminder has also been sent to the renter automatically.</p>";

        return EmailTemplate.Build("Renter payment overdue", body);
    }

    private static string BuildEmailBody(string renterName, string apartmentNumber, decimal amount, DateTime dueDate, int daysUntilDue)
    {
        var timing = daysUntilDue switch
        {
            > 0 => $"is due on {dueDate:MMMM d}",
            0 => "is due today",
            _ => $"was due on {dueDate:MMMM d} and is now overdue"
        };

        var heading = daysUntilDue < 0 ? "Payment overdue" : "Payment reminder";
        var body = $@"
            <p style=""margin:0 0 4px;"">Hi {renterName},</p>
            <p style=""margin:12px 0 0;"">Your apartment rental payment of
            <strong style=""color:#14151F;"">&#8369;{amount:N2}</strong> for apartment {apartmentNumber} {timing}.</p>";

        return EmailTemplate.Build(heading, body);
    }

    private static string BuildReminderMessage(string apartmentNumber, decimal amount, DateTime dueDate, int daysUntilDue)
    {
        var timing = daysUntilDue switch
        {
            > 0 => $"is due on {dueDate:MMM d}",
            0 => "is due today",
            _ => $"was due {dueDate:MMM d} and is now overdue"
        };

        return $"Apartment {apartmentNumber}: your rent payment of PHP {amount:N2} {timing}. Please pay as soon as possible.";
    }
}
