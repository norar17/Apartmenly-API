# Multi-stage build: compile with the full SDK, ship only the runtime layer.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first and restore separately so Docker can cache this
# layer - only re-runs when a .csproj actually changes, not on every code edit.
COPY src/ApartmentRental.Domain/ApartmentRental.Domain.csproj src/ApartmentRental.Domain/
COPY src/ApartmentRental.Shared/ApartmentRental.Shared.csproj src/ApartmentRental.Shared/
COPY src/ApartmentRental.Application/ApartmentRental.Application.csproj src/ApartmentRental.Application/
COPY src/ApartmentRental.Infrastructure/ApartmentRental.Infrastructure.csproj src/ApartmentRental.Infrastructure/
COPY src/ApartmentRental.API/ApartmentRental.API.csproj src/ApartmentRental.API/
RUN dotnet restore src/ApartmentRental.API/ApartmentRental.API.csproj

COPY src/ ./src/
RUN dotnet publish src/ApartmentRental.API/ApartmentRental.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render (and most PaaS platforms) inject $PORT at runtime - Kestrel binds
# to it here instead of the fixed localhost:5001 used for local dev
# (Program.cs only forces that URL in Development).
ENV ASPNETCORE_URLS=http://+:8080

# Constrained containers (Render's free tier especially) hit the OS
# inotify-instance limit almost immediately, because ASP.NET Core's default
# config setup watches appsettings.json for live-reload via FileSystemWatcher.
# This env var is read before WebApplication.CreateBuilder() does anything
# else, so it's the one thing that actually prevents the watcher from being
# created in the first place - Program.cs's own config setup runs too late
# to help (the crash happens inside CreateBuilder itself). Official fix, see
# https://learn.microsoft.com/aspnet/core/host-and-deploy/docker/
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false

EXPOSE 8080

ENTRYPOINT ["dotnet", "ApartmentRental.API.dll"]
