using ApartmentRental.API.Extensions;
using ApartmentRental.API.Filters;
using ApartmentRental.API.Middleware;
using ApartmentRental.Application.DependencyInjection;
using ApartmentRental.Infrastructure.DependencyInjection;
using ApartmentRental.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Configuration
    .AddEnvironmentVariables()
    .AddCommandLine(args);

var isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
if (builder.Environment.IsDevelopment() && !isRunningInContainer)
{
    builder.WebHost.UseUrls("https://localhost:5001");
}

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddApiRateLimiting();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql");

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5173" };

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Apartment Rental API v1");
    });
}

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment() && !isRunningInContainer)
{
    app.UseHttpsRedirection();
}
else
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}

app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await SeedData.MigrateAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    await SeedData.SeedAsync(app.Services);
}

app.Run();

public partial class Program { }