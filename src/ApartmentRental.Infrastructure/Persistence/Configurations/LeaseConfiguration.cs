using ApartmentRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentRental.Infrastructure.Persistence.Configurations;

public class LeaseConfiguration : IEntityTypeConfiguration<Lease>
{
    public void Configure(EntityTypeBuilder<Lease> builder)
    {
        builder.ToTable("Leases");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.MonthlyRent).HasColumnType("decimal(12,2)");
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.TerminationReason).HasMaxLength(500);
        builder.Property(l => l.Notes).HasMaxLength(2000);

        builder.HasMany(l => l.Payments)
            .WithOne(p => p.Lease)
            .HasForeignKey(p => p.LeaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.ApartmentId, l.Status });
        builder.HasIndex(l => new { l.RenterId, l.Status });
    }
}
