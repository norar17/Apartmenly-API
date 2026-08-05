using ApartmentRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentRental.Infrastructure.Persistence.Configurations;

public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.ToTable("Owners");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.FullName).IsRequired().HasMaxLength(150);
        builder.Property(o => o.Email).IsRequired().HasMaxLength(200);
        builder.Property(o => o.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(o => o.PasswordHash).IsRequired();
        builder.Property(o => o.CompanyName).HasMaxLength(150);

        builder.HasIndex(o => o.Email).IsUnique();

        builder.HasMany(o => o.Apartments)
            .WithOne(a => a.Owner)
            .HasForeignKey(a => a.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
