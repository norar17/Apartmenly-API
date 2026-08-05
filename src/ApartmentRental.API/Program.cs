using ApartmentRental.API.Extensions;
using ApartmentRental.API.Filters;
using ApartmentRental.API.Middleware;
using ApartmentRental.Application.DependencyInjection;
using ApartmentRental.Infrastructure.DependencyInjection;
using ApartmentRental.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Render's containers hit the OS inotify-instance limit almost immediately
// because ASP.NET Core's default config setup watches appsettings.json for
// live-reload via FileSystemWatcher. Not needed here - config comes from
// env vars in every deployed environment - so config sources are rebuilt
// without file watching, which is what was actually crashing the app on
// startup (System.IO.IOException: configured user limit on inotify instances).
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

// Fixed port for local (non-container) dev only. Deployment platforms and
// Docker set their own $PORT / ASPNETCORE_URLS, and the official .NET
// container images set DOTNET_RUNNING_IN_CONTAINER=true automatically -
// checking it here means this stays safe even if someone runs the
// container locally with ASPNETCORE_ENVIRONMENT=Development.
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
    // Render (and most PaaS platforms) terminate TLS at their edge and
    // forward plain HTTP to the container - trust their proxy headers so
    // the app still sees the real scheme/host instead of redirect-looping.
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

// Applying migrations is safe (and necessary) in every environment.
// Demo data defaults to Development-only, but can be explicitly turned on
// anywhere (e.g. a portfolio deploy on Render) by setting SeedDemoData=true
// in that environment's config/env vars - useful for e.g. giving reviewers
// a working demo login without exposing full Development mode (Swagger, etc).
await SeedData.MigrateAsync(app.Services);

var shouldSeedDemoData = app.Configuration.GetValue<bool?>("SeedDemoData") ?? app.Environment.IsDevelopment();
if (shouldSeedDemoData)
{
    await SeedData.SeedAsync(app.Services);
}

app.Run();

public partial class Program { }
