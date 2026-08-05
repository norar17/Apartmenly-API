using ApartmentRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApartmentRental.Infrastructure.Persistence.Configurations;

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("EmailLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ToAddress).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Subject).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
    }
}

public class SmsLogConfiguration : IEntityTypeConfiguration<SmsLog>
{
    public void Configure(EntityTypeBuilder<SmsLog> builder)
    {
        builder.ToTable("SmsLogs");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.ToPhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Message).IsRequired().HasMaxLength(500);
        builder.Property(s => s.Provider).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
    }
}

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ActorName).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.EntityType).HasMaxLength(100);

        builder.HasIndex(a => a.CreatedAt);
    }
}
