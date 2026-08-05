using ApartmentRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentRental.Infrastructure.Persistence.Configurations;

public class RenterConfiguration : IEntityTypeConfiguration<Renter>
{
    public void Configure(EntityTypeBuilder<Renter> builder)
    {
        builder.ToTable("Renters");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.FullName).IsRequired().HasMaxLength(150);
        builder.Property(r => r.Email).IsRequired().HasMaxLength(200);
        builder.Property(r => r.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(r => r.PasswordHash).IsRequired();
        builder.Property(r => r.Address).HasMaxLength(500);

        builder.HasIndex(r => r.Email).IsUnique();

        builder.HasMany(r => r.Leases)
            .WithOne(l => l.Renter)
            .HasForeignKey(l => l.RenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.MaintenanceRequests)
            .WithOne(m => m.Renter)
            .HasForeignKey(m => m.RenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
