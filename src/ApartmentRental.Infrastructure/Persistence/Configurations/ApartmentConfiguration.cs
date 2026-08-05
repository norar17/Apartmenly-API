using ApartmentRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentRental.Infrastructure.Persistence.Configurations;

public class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
{
    public void Configure(EntityTypeBuilder<Apartment> builder)
    {
        builder.ToTable("Apartments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ApartmentNumber).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Description).HasMaxLength(2000);
        builder.Property(a => a.MonthlyRent).HasColumnType("decimal(12,2)");
        builder.Property(a => a.Deposit).HasColumnType("decimal(12,2)");
        builder.Property(a => a.SizeSqm).HasColumnType("decimal(8,2)");
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(a => new { a.OwnerId, a.ApartmentNumber }).IsUnique();

        builder.HasMany(a => a.Photos)
            .WithOne(p => p.Apartment)
            .HasForeignKey(p => p.ApartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Leases)
            .WithOne(l => l.Apartment)
            .HasForeignKey(l => l.ApartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.MaintenanceRequests)
            .WithOne(m => m.Apartment)
            .HasForeignKey(m => m.ApartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
