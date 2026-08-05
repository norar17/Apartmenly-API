using ApartmentRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentRental.Infrastructure.Persistence.Configurations;

public class ApartmentPhotoConfiguration : IEntityTypeConfiguration<ApartmentPhoto>
{
    public void Configure(EntityTypeBuilder<ApartmentPhoto> builder)
    {
        builder.ToTable("ApartmentPhotos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Url).IsRequired().HasMaxLength(1000);
    }
}
