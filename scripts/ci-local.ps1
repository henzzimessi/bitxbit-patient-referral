[CmdletBinding()]
param(
    [switch]$SkipDocker,
    [switch]$SkipE2E
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][scriptblock]$Action
    )

    Write-Host "`n==> $Name" -ForegroundColor Cyan
    & $Action
}

function Wait-ForHealth {
    param(
        [Parameter(Mandatory = $true)][string]$Url,
        [int]$Attempts = 60,
        [int]$DelaySeconds = 2
    )

    for ($i = 1; $i -le $Attempts; $i++) {
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -eq 200) {
                Write-Host "Health check OK ($Url)" -ForegroundColor Green
                return
            }
        } catch {
            # ignore and retry
        }

        Write-Host "Waiting for API health... ($i/$Attempts)" -ForegroundColor Yellow
        Start-Sleep -Seconds $DelaySeconds
    }

    throw "API did not become healthy in time: $Url"
}

function Invoke-SmokeTest {
    param(
        [Parameter(Mandatory = $true)][string]$BaseUrl
    )

    $body = @{
        firstName   = 'CI'
        lastName    = 'Smoke'
        dateOfBirth = '1985-06-15'
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Method Post -Uri "$BaseUrl/api/patients" -ContentType 'application/json' -Body $body -UseBasicParsing
    if ($response.StatusCode -ne 201) {
        throw "Expected 201 from POST /api/patients, got $($response.StatusCode)"
    }
}

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$baseUrl = 'http://localhost:8080'
$healthUrl = "$baseUrl/health"

Invoke-Step -Name 'Ensure .env for compose' -Action {
    if (-not (Test-Path '.env.example')) {
        throw '.env.example is missing'
    }
    Copy-Item -Path '.env.example' -Destination '.env' -Force
}

if (-not $SkipDocker) {
    Invoke-Step -Name 'Start Docker stack (db + init + api)' -Action {
        .\dev.cmd up
    }

    Invoke-Step -Name 'Wait for API health' -Action {
        Wait-ForHealth -Url $healthUrl
    }

    Invoke-Step -Name 'Smoke test DB-backed write endpoint' -Action {
        Invoke-SmokeTest -BaseUrl $baseUrl
    }
} else {
    Write-Host "Skipping Docker startup and health checks" -ForegroundColor Yellow
}

Invoke-Step -Name 'Run internal tests (unit + integration)' -Action {
    dotnet test .\tests\PatientReferral.Tests\PatientReferral.Tests.csproj -c Release --logger "console;verbosity=normal"
}

if (-not $SkipE2E) {
    Invoke-Step -Name 'Run external E2E tests' -Action {
        $env:E2E_BASE_URL = $baseUrl
        dotnet test .\tests\PatientReferral.E2E.Tests\PatientReferral.E2E.Tests.csproj -c Release --logger "console;verbosity=normal"
    }
} else {
    Write-Host "Skipping E2E tests" -ForegroundColor Yellow
}

Write-Host "`nLocal CI checks completed successfully." -ForegroundColor Green
