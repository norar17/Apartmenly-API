using ApartmentRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentRental.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ReceiptNumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Amount).HasColumnType("decimal(12,2)");
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Notes).HasMaxLength(1000);
        builder.Property(p => p.ForMonth).HasMaxLength(7);

        builder.HasIndex(p => p.ReceiptNumber).IsUnique();
        builder.HasIndex(p => new { p.LeaseId, p.Status });
        builder.HasIndex(p => p.DueDate);
    }
}
