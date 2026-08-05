using System.Reflection;
using ApartmentRental.Domain.Common;
using ApartmentRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApartmentRental.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Apartment> Apartments => Set<Apartment>();
    public DbSet<ApartmentPhoto> ApartmentPhotos => Set<ApartmentPhoto>();
    public DbSet<Renter> Renters => Set<Renter>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<SmsLog> SmsLogs => Set<SmsLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<MagicLinkToken> MagicLinkTokens => Set<MagicLinkToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Applies a global "WHERE IsDeleted = false" filter to every entity,
        // so soft-deleted rows never appear in normal queries.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
                var lambda = System.Linq.Expressions.Expression.Lambda(condition, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    // PH has no daylight saving, so a flat +8 offset is exact and simpler
    // than a TimeZoneInfo lookup (which also isn't guaranteed to resolve
    // "Asia/Manila" the same way on every OS).
    private static readonly TimeSpan PhilippineOffset = TimeSpan.FromHours(8);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Npgsql refuses to write DateTime.Kind=Unspecified into "timestamp
        // with time zone" columns, and this app is PH-only - so any
        // Unspecified DateTime (e.g. lease start/end dates typed by the
        // user) is treated as Philippine local time and converted to the
        // true UTC instant before saving. Everything is stored as real UTC;
        // the frontend then displays it correctly in the browser's local
        // (PH) time automatically - no further timezone handling needed
        // anywhere else in the app.
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
            {
                continue;
            }

            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTime dateTime && dateTime.Kind != DateTimeKind.Utc)
                {
                    property.CurrentValue = dateTime.Kind == DateTimeKind.Local
                        ? dateTime.ToUniversalTime()
                        : DateTime.SpecifyKind(dateTime - PhilippineOffset, DateTimeKind.Utc);
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
