using ApartmentRental.Application.Common.Interfaces;
using ApartmentRental.Domain.Entities;
using ApartmentRental.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApartmentRental.Infrastructure.Persistence;

// Seeds one owner, one renter, a handful of apartments, an active lease, and
// payment history so the app is demo-ready right after `dotnet ef database update`.
public static class SeedData
{
    // Safe to run in every environment, including production - applies any
    // pending migrations and does nothing else. Called on every startup.
    public static async Task MigrateAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }

    // Development-only demo data. Assumes migrations have already run.
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await context.Owners.AnyAsync()) return; // already seeded

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var owner = new Owner
        {
            FullName = "Maria Santos",
            Email = "owner@demo.com",
            PhoneNumber = "+639171234567",
            PasswordHash = passwordHasher.Hash("Password123!"),
            CompanyName = "Santos Apartments",
            EmailVerifiedAt = DateTime.UtcNow
        };
        context.Owners.Add(owner);

        var apartments = new List<Apartment>
        {
            new() { OwnerId = owner.Id, ApartmentNumber = "101", Floor = 1, MonthlyRent = 8500, Deposit = 17000, Bedrooms = 1, Bathrooms = 1, Status = ApartmentStatus.Occupied, Description = "Cozy unit near the entrance." },
            new() { OwnerId = owner.Id, ApartmentNumber = "102", Floor = 1, MonthlyRent = 9000, Deposit = 18000, Bedrooms = 1, Bathrooms = 1, Status = ApartmentStatus.Available, Description = "Bright corner unit." },
            new() { OwnerId = owner.Id, ApartmentNumber = "201", Floor = 2, MonthlyRent = 12000, Deposit = 24000, Bedrooms = 2, Bathrooms = 1, Status = ApartmentStatus.Available, Description = "Spacious two bedroom." },
            new() { OwnerId = owner.Id, ApartmentNumber = "202", Floor = 2, MonthlyRent = 12500, Deposit = 25000, Bedrooms = 2, Bathrooms = 2, Status = ApartmentStatus.Maintenance, Description = "Under repair after plumbing issue." }
        };
        context.Apartments.AddRange(apartments);

        var renter = new Renter
        {
            FullName = "Juan Dela Cruz",
            Email = "renter@demo.com",
            PhoneNumber = "+639181234567",
            PasswordHash = passwordHasher.Hash("Password123!"),
            Address = "Tagudin, Ilocos Sur",
            EmergencyContactName = "Ana Dela Cruz",
            EmergencyContactPhone = "+639191234567",
            MoveInDate = DateTime.UtcNow.AddMonths(-3),
            EmailVerifiedAt = DateTime.UtcNow
        };
        context.Renters.Add(renter);

        var lease = new Lease
        {
            ApartmentId = apartments[0].Id,
            RenterId = renter.Id,
            MonthlyRent = 8500,
            DueDay = 10,
            StartDate = DateTime.UtcNow.AddMonths(-3),
            EndDate = DateTime.UtcNow.AddMonths(9),
            Status = LeaseStatus.Active
        };
        context.Leases.Add(lease);

        var payments = new List<Payment>
        {
            new() { LeaseId = lease.Id, ReceiptNumber = "RCT-SEED-0001", Amount = 8500, DueDate = DateTime.UtcNow.AddMonths(-2), PaymentDate = DateTime.UtcNow.AddMonths(-2), PaymentMethod = PaymentMethod.GCash, Status = PaymentStatus.Paid, ForMonth = DateTime.UtcNow.AddMonths(-2).ToString("yyyy-MM") },
            new() { LeaseId = lease.Id, ReceiptNumber = "RCT-SEED-0002", Amount = 8500, DueDate = DateTime.UtcNow.AddMonths(-1), PaymentDate = DateTime.UtcNow.AddMonths(-1), PaymentMethod = PaymentMethod.GCash, Status = PaymentStatus.Paid, ForMonth = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM") },
            new() { LeaseId = lease.Id, ReceiptNumber = "RCT-SEED-0003", Amount = 8500, DueDate = DateTime.UtcNow.AddDays(5), PaymentMethod = PaymentMethod.GCash, Status = PaymentStatus.Pending, ForMonth = DateTime.UtcNow.ToString("yyyy-MM") }
        };
        context.Payments.AddRange(payments);

        context.MaintenanceRequests.Add(new MaintenanceRequest
        {
            ApartmentId = apartments[0].Id,
            RenterId = renter.Id,
            Title = "Leaking faucet",
            Description = "The kitchen faucet has been dripping for two days.",
            Priority = MaintenancePriority.Medium,
            Status = MaintenanceStatus.Pending
        });

        context.ActivityLogs.Add(new ActivityLog
        {
            ActorName = owner.FullName,
            Action = "System.Seeded",
            Description = "Initial demo data was seeded."
        });

        await context.SaveChangesAsync();
    }
}
