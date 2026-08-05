using ApartmentRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentRental.Infrastructure.Persistence.Configurations;

public class MagicLinkTokenConfiguration : IEntityTypeConfiguration<MagicLinkToken>
{
    public void Configure(EntityTypeBuilder<MagicLinkToken> builder)
    {
        builder.ToTable("MagicLinkTokens");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Email).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Purpose).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Token).IsRequired().HasMaxLength(200);

        builder.HasIndex(m => m.Token).IsUnique();
        builder.HasIndex(m => new { m.Email, m.Role });
    }
}
