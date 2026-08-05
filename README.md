# Apartment Rental Management System — Backend

A production-ready **Apartment Rental Management System** built with **ASP.NET Core 10**, **PostgreSQL**, and **Feature-Folder Clean Architecture**. The project follows a scalable architecture using the Repository and Unit of Work patterns, JWT authentication, background services, and a consistent API response structure, making it suitable for real-world applications and portfolio projects.

> **Tech Stack:** ASP.NET Core 10 • Entity Framework Core • PostgreSQL • JWT • FluentValidation • Serilog • Docker • Render • Resend Email

---

# Architecture

* Feature-folder architecture
* Clean Architecture
* Generic Repository Pattern
* Unit of Work Pattern
* Repository<T>() accessor
* ApiResponse<T> response wrapper
* PagedResult<T> pagination
* JWT Authentication & Refresh Tokens
* BCrypt Password Hashing
* FluentValidation
* Background Services
* Dependency Injection
* Health Checks
* Rate Limiting
* Serilog Logging
* Docker Support
* Render Deployment Ready

---

# Project Structure

The solution consists of **5 projects**:

```
ApartmentRental.sln
│
├── ApartmentRental.API
├── ApartmentRental.Application
├── ApartmentRental.Domain
├── ApartmentRental.Infrastructure
└── ApartmentRental.Shared
```

Persistence is integrated into the **Infrastructure** project to keep the solution simple while maintaining a clean separation of concerns.

Application logic is organized by **feature** instead of technical layers.

Example:

```
Application
│
├── Apartments
│   ├── DTOs
│   ├── Interfaces
│   ├── Services
│   └── Validators
│
├── Renters
├── Leases
├── Payments
├── Dashboard
├── Reports
└── Notifications
```

---

# Setup

Clone the repository and restore dependencies.

```bash
dotnet restore
dotnet build
```

Navigate to the API project.

```bash
cd src/ApartmentRental.API
```

Create an **appsettings.Development.json** file.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=apartment_rental;Username=postgres;Password=YOUR_DB_PASSWORD"
  },

  "Jwt": {
    "Secret": "REPLACE_WITH_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARACTERS"
  },

  "Email": {
    "Provider": "Resend",
    "SenderName": "Apartment Management",
    "SenderEmail": "noreply@yourdomain.com",
    "ResendApiKey": "re_your_api_key_here"
  },

  "Sms": {
    "Provider": "Console"
  }
}
```

---

# Database Migration

Generate the initial migration.

```bash
dotnet ef migrations add InitialCreate --project ../ApartmentRental.Infrastructure --startup-project .
```

Apply the migration.

```bash
dotnet ef database update --project ../ApartmentRental.Infrastructure --startup-project .
```

---

# Run the Application

```bash
dotnet run
```

Swagger is available at:

```
/swagger
```

---

# Demo Accounts

| Role   | Email                                     | Password     |
| ------ | ----------------------------------------- | ------------ |
| Owner  | [owner@demo.com](mailto:owner@demo.com)   | Password123! |
| Renter | [renter@demo.com](mailto:renter@demo.com) | Password123! |

---

# Testing Email

The project includes a development endpoint for sending a real email without waiting for the scheduled reminder service.

```
POST /api/devtools/test-email
```

Request

```json
{
  "email": "you@example.com",
  "subject": "Test",
  "message": "Test email from Apartment Rental"
}
```

The response includes whether the email was successfully delivered along with any provider error message.

---

# Features

### Authentication

* JWT Authentication
* Refresh Tokens
* BCrypt Password Hashing
* Role-Based Authorization

---

### Apartment Management

* Apartment CRUD
* Unit Management
* Occupancy Tracking
* Availability Status

---

### Renter Management

* Tenant Registration
* Tenant Profiles
* Contact Information
* Active Lease Tracking

---

### Lease Management

* Lease Creation
* Lease Renewal
* Lease Termination
* Rental Agreement Tracking

---

### Payment Management

* Payment Recording
* Payment History
* Due Date Tracking
* Monthly Payment Status

---

### Notifications

* Email Reminders
* Payment Due Notifications
* Background Reminder Service

---

### Reports

* Dashboard Statistics
* Occupancy Reports
* Revenue Reports
* Payment Reports

---

### Security

* JWT Authentication
* Refresh Tokens
* Password Hashing
* Rate Limiting
* CORS
* Health Checks

---

### Logging

* Serilog
* Activity Logs
* Audit Trail

---

### Validation

FluentValidation is automatically executed through a global validation filter. Every incoming request is validated before reaching the service layer. Validation failures return a consistent **400 Bad Request** response wrapped inside `ApiResponse<T>`.

---

# API Response Format

Every endpoint returns a consistent response structure.

```json
{
  "success": true,
  "message": "Request completed successfully.",
  "data": {},
  "errorCode": null,
  "errors": [],
  "timestampUtc": "2026-08-05T12:00:00Z"
}
```

Paginated endpoints return a `PagedResult<T>` inside the `data` property.

---

# Docker

Start PostgreSQL and the API.

```bash
docker compose up --build
```

Swagger

```
http://localhost:8080/swagger
```

Build only the API image.

```bash
docker build -t apartment-rental-api .
```

Run the container.

```bash
docker run -p 8080:8080 apartment-rental-api
```

---

# Deploying to Render

1. Push the repository to GitHub, GitLab, or Bitbucket.
2. Create a new **Blueprint** in Render.
3. Connect your repository.
4. Configure the required environment variables.
5. Deploy.

The application automatically applies database migrations during startup.

Health checks are available at:

```
/health
```

---

# Deploying the Frontend to Vercel

1. Import the repository into Vercel.
2. Set the **Root Directory** to the frontend project.
3. Add the environment variable:

```
VITE_API_BASE_URL=https://your-api.onrender.com/api
```

4. Deploy the project.
5. Update the backend CORS settings with your Vercel domain.

---

# Technologies

* ASP.NET Core 10
* Entity Framework Core
* PostgreSQL
* JWT Authentication
* FluentValidation
* Serilog
* Docker
* Render
* Vercel
* Resend Email
* Swagger (OpenAPI)

---

# License

This project is intended for educational, learning, and portfolio purposes.
