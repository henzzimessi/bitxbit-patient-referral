# Patient Referral Management API

## Overview

A small REST API for managing patients and referral notes using ASP.NET Core 8 and SQL Server.

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB, SQL Express, or Docker SQL Server)

## Database Setup

Run the SQL scripts in order:

1. `sql/create_tables.sql`
2. (Optional) `sql/seed_data.sql`

### Docker (one command SQL Server setup)

This repo includes a `docker-compose.yml`, `sql/init.sh`, and API Dockerfile to start SQL Server, apply schema/seed, and run the API in one command.

1. Update the SA password in `.env` (required).
2. Start everything (DB + init + API):

```
docker compose up --build -d
```

SQL Server will be available on `localhost,1433` and the API on `http://localhost:8080`.
Swagger UI: `http://localhost:8080/swagger`
Health endpoint: `http://localhost:8080/health`

### Simplified commands

From the repo root, use a single command entrypoint:

- `dev.cmd up` → build and start DB + init + API
- `dev.cmd down` → stop containers (keeps DB volume)
- `dev.cmd reset` → recreate containers and reset DB volume
- `dev.cmd status` → show running services
- `dev.cmd logs` → show service logs
- `dev.cmd test` → run all unit + integration tests
- `dev.cmd test-e2e` → run external E2E tests against running Docker backend
- `dev.cmd test-all` → run internal tests + external E2E tests
- `dev.cmd ci` → run full local CI workflow (docker + health + smoke + tests)

PowerShell helper (equivalent):

- `./scripts/stack.ps1 up`
- `./scripts/stack.ps1 down`
- `./scripts/stack.ps1 reset`
- `./scripts/stack.ps1 status`
- `./scripts/stack.ps1 logs`
- `./scripts/stack.ps1 test`
- `./scripts/stack.ps1 test-e2e`
- `./scripts/stack.ps1 test-all`
- `./scripts/stack.ps1 ci`

Help:

- `dev.cmd help`
- `./scripts/stack.ps1 help`

Optional `make` targets (if `make` is installed):

- `make up`
- `make down`
- `make reset`
- `make status`
- `make logs`
- `make test`
- `make test-e2e`
- `make test-all`
- `make ci`

## Configuration

Edit the connection string in `src/PatientReferral.Api/appsettings.json` or use environment variables/user‑secrets.

Example environment variable for Docker SQL Server:

```
ConnectionStrings__DefaultConnection=Server=localhost,1433;Database=PatientReferralDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
```

## Run the API

From the repository root:

- Build: `dotnet build`
- Run: `dotnet run --project src/PatientReferral.Api`

Swagger UI is available in Development at `/swagger`.

## Endpoints

- `POST /api/patients`
- `POST /api/referrals`
- `GET /api/referrals/{id}`
- `GET /api/patients/{id}/referrals?page=1&pageSize=10`
- `GET /health`

## Example Requests

Create patient:

```json
{
  "firstName": "John",
  "lastName": "Smith",
  "dateOfBirth": "1943-02-01"
}
```

Successful response (`201 Created`):

```json
{
  "patientId": 1,
  "firstName": "John",
  "lastName": "Smith",
  "dateOfBirth": "1943-02-01",
  "createdDate": "2026-03-23T12:00:00Z"
}
```

Create referral:

```json
{
  "patientId": 1,
  "referralSource": "Hospital",
  "referralType": "Short Stay",
  "referralNote": "Patient recently hospitalized with pneumonia."
}
```

Successful response (`201 Created`):

```json
{
  "referralId": 1,
  "patientId": 1,
  "referralSource": "Hospital",
  "referralType": "Short Stay",
  "referralNote": "Patient recently hospitalized with pneumonia.",
  "createdDate": "2026-03-23T12:01:00Z"
}
```

Validation failure example (`400 Bad Request`):

```json
{
  "type": "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "ReferralType": ["'Referral Type' must not be empty."]
  }
}
```

Missing resource example (`404 Not Found`):

```json
{
  "type": "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.5",
  "title": "Patient with id 999 was not found.",
  "status": 404
}
```

Get referrals for a patient (paged response):

```json
{
  "items": [
    {
      "referralId": 1,
      "patientId": 1,
      "referralSource": "Hospital",
      "referralType": "Short Stay",
      "referralNote": "Patient recently hospitalized with pneumonia.",
      "createdDate": "2024-03-21T16:30:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 23,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

## Tests

Run all tests:

- `dotnet test`
- `dev.cmd test`
- `make test`

Run external E2E tests (requires backend running at `http://localhost:8080`):

- `dev.cmd test-e2e`
- `make test-e2e`

Run everything:

- `dev.cmd test-all`
- `make test-all`

### Local CI (pre-push)

To avoid CI surprises, run the same checks locally that GitHub Actions runs:

- `./scripts/ci-local.ps1`

Optional flags:

- `./scripts/ci-local.ps1 -SkipDocker` (use when the stack is already running)
- `./scripts/ci-local.ps1 -SkipE2E` (skip external E2E tests)

Notes:

- The current integration tests (`tests/PatientReferral.Tests/Integration`) run HTTP-level API tests through `WebApplicationFactory` with EF Core InMemory.
- They are fully automated and run as part of the same test command above.
- External E2E tests are in `tests/PatientReferral.E2E.Tests` and hit the live API URL (`E2E_BASE_URL`, default: `http://localhost:8080`) with real SQL Server state.

## CI (GitHub Actions)

This repository includes a CI workflow at `.github/workflows/ci.yml`.

On push / pull request (main or master), CI will:

1. Restore the .NET solution
2. Prepare `.env` from `.env.example` for Docker Compose
3. Build and start Docker stack (`db`, `db-init`, `api`)
4. Wait for `db-init` to complete successfully
5. Wait for API readiness using `GET /health`
6. Run a DB-backed smoke test against `POST /api/patients`
7. Run internal tests (`tests/PatientReferral.Tests`)
8. Run external E2E tests (`tests/PatientReferral.E2E.Tests`)
9. Print docker logs and upload a compose log artifact on failure
10. Shutdown compose and remove volumes

## Troubleshooting

- If terminal opens with repeated `PS ...>` prompts in an infinite loop, this workspace is configured to use `PowerShell (NoProfile)` to bypass broken user profile scripts.
- Close all terminals and open a new one after pulling latest changes.
