using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ApartmentRental.Infrastructure.Persistence;

// Lets `dotnet ef migrations add ...` build the DbContext without spinning
// up the full API host. Reads the SAME appsettings.json /
// appsettings.Development.json the real app uses (found relative to the
// current directory, i.e. wherever `dotnet ef` was run from - normally
// src/ApartmentRental.API) so there's nothing extra to configure. The
// DEFAULT_CONNECTION_STRING env var still works too, as a last-resort override.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=apartment_rental;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
