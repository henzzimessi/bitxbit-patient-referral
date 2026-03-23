param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("up", "down", "reset", "status", "logs", "test", "test-e2e", "test-all", "ci", "help")]
    [string]$Action
)

$ErrorActionPreference = "Stop"

if (-not $Action -or $Action -eq "help") {
    Write-Host "Usage: ./scripts/stack.ps1 <action>" -ForegroundColor Cyan
    Write-Host "Actions: up | down | reset | status | logs | test | test-e2e | test-all | ci" -ForegroundColor Gray
    exit 0
}

switch ($Action) {
    "up" {
        Write-Host "Starting DB + init + API..." -ForegroundColor Cyan
        docker compose up --build -d
        break
    }
    "down" {
        Write-Host "Stopping containers (keeping data volume)..." -ForegroundColor Yellow
        docker compose down
        break
    }
    "reset" {
        Write-Host "Resetting containers and database volume..." -ForegroundColor Yellow
        docker compose down -v
        docker compose up --build -d
        break
    }
    "status" {
        docker compose ps
        break
    }
    "logs" {
        docker compose logs -f
        break
    }
    "test" {
        dotnet test .\tests\PatientReferral.Tests\PatientReferral.Tests.csproj -c Release
        break
    }
    "test-e2e" {
        dotnet test .\tests\PatientReferral.E2E.Tests\PatientReferral.E2E.Tests.csproj -c Release
        break
    }
    "test-all" {
        dotnet test .\tests\PatientReferral.Tests\PatientReferral.Tests.csproj -c Release
        dotnet test .\tests\PatientReferral.E2E.Tests\PatientReferral.E2E.Tests.csproj -c Release
        break
    }
    "ci" {
        .\scripts\ci-local.ps1
        break
    }
}
