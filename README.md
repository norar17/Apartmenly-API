<<<<<<< HEAD
# Apartment Rental Management System — Backend (v2, feature-folder architecture)

Restructured to follow the architecture of [norar17/Nova-Api](https://github.com/norar17/Nova-Api):
5 projects instead of 6 (Persistence merged into Infrastructure), feature
folders instead of layer folders, a generic `Repository<T>()` accessor on
`IUnitOfWork` instead of one property per entity, and a consistent
`ApiResponse<T>` envelope on every endpoint.

> Built without a .NET SDK in this environment, so it has **not** been
> compiled here — same caveat as the original drop. Budget time for a first
> `dotnet build` pass.

---

## ⚠️ This replaces the earlier zip — you need to re-migrate

This is a different project shape than the first backend you set up
(different namespaces, `Persistence` folder moved into `Infrastructure`,
`Migrations` folder moved too). **Don't try to reuse your existing
migration/database** — start fresh:

```bash
dotnet ef database drop --project ../ApartmentRental.Infrastructure --startup-project . --force
```
(only if you still have the old database around — otherwise skip straight to the setup below.)

> **If you already migrated this v2 backend once:** you don't need to drop
> anything again. Just run
> `dotnet ef migrations add AddForgotPassword --project ../ApartmentRental.Infrastructure --startup-project .`
> followed by `dotnet ef database update ...` and you're current. (Adds a
> `Purpose` column to `MagicLinkTokens` so sign-in links and password-reset
> links can't be swapped for each other.)

## 1. Setup

```bash
cd ApartmentRental
=======
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
>>>>>>> 7c6eacdc49bd17e161b7903924de304e15c7d12d
dotnet restore
dotnet build
```

<<<<<<< HEAD
```bash
cd src/ApartmentRental.API
```
Create `appsettings.Development.json` yourself in that folder (it's git-ignored, so it's never committed) with:
=======
Navigate to the API project.

```bash
cd src/ApartmentRental.API
```

Create an **appsettings.Development.json** file.
>>>>>>> 7c6eacdc49bd17e161b7903924de304e15c7d12d

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=apartment_rental;Username=postgres;Password=YOUR_DB_PASSWORD"
  },
<<<<<<< HEAD
  "Jwt": {
    "Secret": "REPLACE_WITH_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS"
  },
=======

  "Jwt": {
    "Secret": "REPLACE_WITH_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARACTERS"
  },

>>>>>>> 7c6eacdc49bd17e161b7903924de304e15c7d12d
  "Email": {
    "Provider": "Resend",
    "SenderName": "Apartment Management",
    "SenderEmail": "noreply@yourdomain.com",
    "ResendApiKey": "re_your_api_key_here"
  },
<<<<<<< HEAD
=======

>>>>>>> 7c6eacdc49bd17e161b7903924de304e15c7d12d
  "Sms": {
    "Provider": "Console"
  }
}
```

<<<<<<< HEAD
**SMS is disabled for now** — the reminder flow and DevTools only send email.
`Sms:Provider: "Console"` is a no-op that just logs instead of sending. The
Twilio/Semaphore/TextBee provider code is still there and works if you want
to wire it back in later (`Infrastructure/Sms/`), set `Sms:Provider`
accordingly and re-add the `ISmsService` call where you want it — it's just
not called from anywhere in the app right now.

## 2. Migrations

Note the `--project` path now points at **Infrastructure**, not Persistence — that project no longer exists.

```bash
dotnet ef migrations add InitialCreate --project ../ApartmentRental.Infrastructure --startup-project .
dotnet ef database update --project ../ApartmentRental.Infrastructure --startup-project .
```

## 3. Run
=======
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
>>>>>>> 7c6eacdc49bd17e161b7903924de304e15c7d12d

```bash
dotnet run
```

<<<<<<< HEAD
Swagger at `/swagger`. Seeded demo accounts (same as before):

| Role   | Email          | Password      |
|--------|----------------|---------------|
| Owner  | owner@demo.com | Password123!  |
| Renter | renter@demo.com| Password123!  |

### Testing email without waiting for the daily reminder sweep

`POST /api/devtools/test-email` (Owner-only — log in as `owner@demo.com`,
authorize with the returned token in Swagger) triggers a real send on demand
and reports back whether it actually succeeded:

```json
POST /api/devtools/test-email
{ "email": "you@example.com", "subject": "Test", "message": "Test email from Apartly" }
```

Response includes `sent: true/false` and the provider's `errorMessage` if it
failed — no more guessing from a generic "request accepted" response.

## 4. What changed vs. the original drop

| | Before | Now |
|---|---|---|
| Projects | 6 (separate Persistence) | 5 (Persistence lives inside Infrastructure) |
| Application organization | One folder per layer (DTOs/, Services/, Interfaces/, Validators/) with all features mixed together | One folder **per feature** (Apartments/, Renters/, Leases/, ...), each with its own DTOs/Interfaces/Services/Validators. Small features (Notifications, Dashboard, Reports) are a single `XxxFeature.cs` file with multiple `namespace` blocks, mirroring the reference repo's `AddressFeature.cs` style |
| Repository access | `_unitOfWork.Apartments`, `_unitOfWork.Renters`, ... (one property per entity) | `_unitOfWork.Repository<Apartment>()`, `_unitOfWork.Repository<Renter>()`, ... (generic, nothing to add when a new entity shows up) |
| `IRepository<T>` / `IUnitOfWork` location | Application layer | **Domain** layer (`Domain/Interfaces/`) — Domain owns the persistence contract |
| Result type | `Result` / `Result<T>` with a plain error string | `Result` / `Result<T>` with `Error` **and** `ErrorCode` (`"NOT_FOUND"`, `"DUPLICATE"`, etc.), plus an implicit `T → Result<T>` conversion |
| API responses | Bare DTOs / `PaginatedList<T>` | Every endpoint wrapped in `ApiResponse<T> { success, data, message, errorCode, errors, timestampUtc }`; lists use `PagedResult<T>` |
| Exceptions | Several files in `Shared/Exceptions/` | One `Shared/Exceptions.cs` with an `AppException` base carrying its own `StatusCode` + `ErrorCode` |
| DI registration | `DependencyInjection.cs` static class per project | `DependencyInjection/ApplicationServiceRegistration.cs` and `DependencyInjection/InfrastructureServiceRegistration.cs`, matching the reference repo's naming |
| Validation | FluentValidation validators registered but never actually invoked (a gap I found in the reference repo itself — see below) | Real validation: `Filters/ValidationFilter.cs` runs the matching validator for every action argument before the controller executes |

## 5. One deviation from the reference repo, on purpose

While porting the pattern, I found that in Nova-Api, FluentValidation
validators are registered in DI (`AddValidatorsFromAssembly`) but are never
actually called anywhere — no filter, no manual `ValidateAsync()` in any
controller. They're dead code; invalid requests reach the service layer
unvalidated. I didn't replicate that gap here: `API/Filters/ValidationFilter.cs`
resolves and runs the right `IValidator<T>` for every controller action
argument, throwing a `ValidationAppException` (→ clean 400 `ApiResponse`) on
failure. Everything else follows the reference structure as closely as made
sense for this domain.

## 6. Everything else

Unchanged from the previous drop: soft deletes, activity log/audit trail,
JWT + refresh tokens, BCrypt hashing, Resend email (SMS infra present but disconnected - see §1),
the daily payment-reminder background service, Swagger with JWT auth,
Serilog, health checks, rate limiting, CORS. See the code comments in each
feature folder — they're the same business logic as before, just relocated.

## 7. Docker

```bash
cd "Rental System"          # the folder containing both backend/ and frontend/
docker compose up --build
```

Starts Postgres + the API together (`docker-compose.yml` at the repo root).
The API listens on `http://localhost:8080` — Swagger at `http://localhost:8080/swagger`
(Development environment, so it's enabled). Demo data seeds automatically,
same as running locally without Docker.

To build just the API image standalone (e.g. to push somewhere yourself):

```bash
cd backend
docker build -t apartment-rental-api .
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="..." apartment-rental-api
```

## 8. Deploying to Render

The repo root has a `render.yaml` Blueprint that provisions the API + a free
Postgres database together:

1. Push the whole `Rental System` folder to a Git repo (GitHub/GitLab/Bitbucket).
2. In the Render dashboard: **New > Blueprint**, connect the repo.
3. Render finds `render.yaml` and shows you every service it's about to
   create. You'll be prompted to fill in the secrets marked `sync: false`
   in the file: `Jwt__Secret`, `Email__SenderEmail`, `Email__ResendApiKey`,
   `Cors__AllowedOrigins__0`, `Frontend__BaseUrl` (the last two are your
   Vercel frontend URL — see §9 below; you can leave them blank and update
   after the frontend is deployed).
4. **Deploy Blueprint.** Render builds `backend/Dockerfile`, provisions the
   database, and wires the connection string in automatically
   (`fromDatabase` in `render.yaml` — no manual copy-pasting a connection
   string).
5. Migrations apply automatically on every startup (`SeedData.MigrateAsync`
   in `Program.cs` runs regardless of environment) — no separate migration
   step needed after the first deploy or any future one.

Health check is wired to `/health`, so Render can tell if a deploy is
actually up before routing traffic to it.

**Free tier note:** Render's free web services spin down after 15 minutes
of inactivity and take ~30-60s to wake back up on the next request — normal
for a portfolio project, just don't be surprised by the first request being
slow after it's been idle.

## 9. Deploying the frontend to Vercel

1. Push (or reuse the same repo — Vercel can point at the `frontend`
   subfolder).
2. In Vercel: **New Project**, import the repo, set **Root Directory** to
   `frontend`. Vercel auto-detects Vite; no build command changes needed.
3. Add an environment variable: `VITE_API_BASE_URL` = your Render API URL
   + `/api`, e.g. `https://apartment-rental-api.onrender.com/api`.
4. Deploy. `frontend/vercel.json` handles SPA routing (so refreshing
   `/owner/dashboard` doesn't 404).
5. Go back to Render and set `Cors__AllowedOrigins__0` and
   `Frontend__BaseUrl` to your new Vercel URL (e.g.
   `https://your-app.vercel.app`), then redeploy the API so CORS and
   magic-link/reset-password emails point at the right place.
=======
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
>>>>>>> 7c6eacdc49bd17e161b7903924de304e15c7d12d
