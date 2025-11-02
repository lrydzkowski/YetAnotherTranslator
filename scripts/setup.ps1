#!/usr/bin/env pwsh
#Requires -Version 7.0

<#
.SYNOPSIS
    Setup script for YetAnotherTranslator
.DESCRIPTION
    Checks prerequisites, configures the database, and validates the environment.
#>

param(
    [switch]$SkipDbSetup,
    [switch]$SkipAzureCheck
)

$ErrorActionPreference = "Stop"

Write-Host "YetAnotherTranslator Setup" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green
Write-Host ""

function Test-Command {
    param([string]$Command)
    $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
}

function Test-DotNetVersion {
    if (-not (Test-Command "dotnet")) {
        Write-Host "❌ .NET SDK not found" -ForegroundColor Red
        Write-Host "   Install from: https://dotnet.microsoft.com/download/dotnet/10.0" -ForegroundColor Yellow
        return $false
    }

    $version = dotnet --version
    if ($version -notmatch "^10\.") {
        Write-Host "❌ .NET 10 RC required (found: $version)" -ForegroundColor Red
        Write-Host "   Install from: https://dotnet.microsoft.com/download/dotnet/10.0" -ForegroundColor Yellow
        return $false
    }

    Write-Host "✓ .NET SDK $version" -ForegroundColor Green
    return $true
}

function Test-PostgreSQL {
    if (-not (Test-Command "psql")) {
        Write-Host "⚠ PostgreSQL client not found in PATH" -ForegroundColor Yellow
        Write-Host "   Install from: https://www.postgresql.org/download/" -ForegroundColor Yellow
        return $false
    }

    Write-Host "✓ PostgreSQL client found" -ForegroundColor Green
    return $true
}

function Test-AzureCli {
    if ($SkipAzureCheck) {
        Write-Host "⊘ Azure CLI check skipped" -ForegroundColor Gray
        return $true
    }

    if (-not (Test-Command "az")) {
        Write-Host "⚠ Azure CLI not found" -ForegroundColor Yellow
        Write-Host "   Install from: https://docs.microsoft.com/cli/azure/install-azure-cli" -ForegroundColor Yellow
        return $false
    }

    try {
        $account = az account show 2>&1 | ConvertFrom-Json
        if ($account) {
            Write-Host "✓ Azure CLI authenticated ($($account.user.name))" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "⚠ Azure CLI not authenticated" -ForegroundColor Yellow
        Write-Host "   Run: az login" -ForegroundColor Yellow
        return $false
    }

    return $false
}

function Test-Configuration {
    $configPath = "$PSScriptRoot/../src/YetAnotherTranslator.Cli/appsettings.json"

    if (-not (Test-Path $configPath)) {
        Write-Host "❌ appsettings.json not found" -ForegroundColor Red
        Write-Host "   Copy from appsettings.example.json and configure" -ForegroundColor Yellow
        return $false
    }

    $config = Get-Content $configPath | ConvertFrom-Json

    $valid = $true

    if ([string]::IsNullOrWhiteSpace($config.Azure.KeyVaultUrl)) {
        Write-Host "❌ Azure Key Vault URL not configured" -ForegroundColor Red
        $valid = $false
    }

    if ([string]::IsNullOrWhiteSpace($config.Database.ConnectionString)) {
        Write-Host "❌ Database connection string not configured" -ForegroundColor Red
        $valid = $false
    }

    if ([string]::IsNullOrWhiteSpace($config.ElevenLabs.DefaultVoiceId)) {
        Write-Host "❌ ElevenLabs voice ID not configured" -ForegroundColor Red
        $valid = $false
    }

    if ($valid) {
        Write-Host "✓ Configuration file valid" -ForegroundColor Green
    }

    return $valid
}

function Setup-Database {
    if ($SkipDbSetup) {
        Write-Host "⊘ Database setup skipped" -ForegroundColor Gray
        return $true
    }

    Write-Host ""
    Write-Host "Database Setup" -ForegroundColor Cyan
    Write-Host "--------------" -ForegroundColor Cyan

    $infraProject = "$PSScriptRoot/../src/YetAnotherTranslator.Infrastructure"
    $cliProject = "$PSScriptRoot/../src/YetAnotherTranslator.Cli"

    Write-Host "Applying EF Core migrations..." -ForegroundColor Gray

    try {
        dotnet ef database update `
            --project $infraProject `
            --startup-project $cliProject `
            --no-build 2>&1 | Out-Null

        Write-Host "✓ Database migrations applied" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "❌ Failed to apply migrations" -ForegroundColor Red
        Write-Host "   Error: $_" -ForegroundColor Yellow
        Write-Host "   Make sure PostgreSQL is running and connection string is correct" -ForegroundColor Yellow
        return $false
    }
}

# Main execution
Write-Host "Checking Prerequisites" -ForegroundColor Cyan
Write-Host "---------------------" -ForegroundColor Cyan

$checks = @{
    "DotNet" = Test-DotNetVersion
    "PostgreSQL" = Test-PostgreSQL
    "AzureCli" = Test-AzureCli
    "Configuration" = Test-Configuration
}

Write-Host ""

$allPassed = $true
foreach ($check in $checks.GetEnumerator()) {
    if (-not $check.Value) {
        $allPassed = $false
    }
}

if (-not $allPassed) {
    Write-Host ""
    Write-Host "❌ Some prerequisites are missing" -ForegroundColor Red
    Write-Host "   Fix the issues above and run setup again" -ForegroundColor Yellow
    exit 1
}

# Build the solution
Write-Host ""
Write-Host "Building Solution" -ForegroundColor Cyan
Write-Host "----------------" -ForegroundColor Cyan

try {
    dotnet build "$PSScriptRoot/../YetAnotherTranslator.slnx" --configuration Release | Out-Null
    Write-Host "✓ Solution built successfully" -ForegroundColor Green
}
catch {
    Write-Host "❌ Build failed" -ForegroundColor Red
    Write-Host "   Run 'dotnet build' for detailed errors" -ForegroundColor Yellow
    exit 1
}

# Setup database
if (-not (Setup-Database)) {
    Write-Host ""
    Write-Host "⚠ Database setup incomplete" -ForegroundColor Yellow
    Write-Host "  You may need to set it up manually" -ForegroundColor Yellow
}

# Final summary
Write-Host ""
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "==============" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Ensure your Azure Key Vault contains:" -ForegroundColor White
Write-Host "   - anthropic-api-key" -ForegroundColor Gray
Write-Host "   - elevenlabs-api-key" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Run the application:" -ForegroundColor White
Write-Host "   cd src/YetAnotherTranslator.Cli" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "For help, see README.md" -ForegroundColor White
