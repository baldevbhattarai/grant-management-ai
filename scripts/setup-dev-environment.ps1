# Grant Management AI - Development Environment Setup
# This script sets up the development environment

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Grant Management AI - Dev Environment Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check .NET SDK
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ .NET SDK installed: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "✗ .NET SDK not found. Please install .NET 8 SDK" -ForegroundColor Red
    exit 1
}

# Check Node.js
$nodeVersion = node --version 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Node.js installed: $nodeVersion" -ForegroundColor Green
} else {
    Write-Host "✗ Node.js not found. Please install Node.js 18+" -ForegroundColor Red
    exit 1
}

# Check SQL Server
Write-Host "✓ Checking SQL Server connection..." -ForegroundColor Yellow
$sqlCheck = sqlcmd -S localhost -Q "SELECT @@VERSION" -b 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ SQL Server is accessible" -ForegroundColor Green
} else {
    Write-Host "✗ Cannot connect to SQL Server. Please ensure SQL Server is running" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "All prerequisites met!" -ForegroundColor Green
Write-Host ""

# Setup database
Write-Host "Setting up database..." -ForegroundColor Yellow
$dbPath = Join-Path $PSScriptRoot "..\database\00_MasterSetup.sql"
if (Test-Path $dbPath) {
    sqlcmd -S localhost -E -i $dbPath
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Database setup complete" -ForegroundColor Green
    } else {
        Write-Host "✗ Database setup failed" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✗ Database setup script not found" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Configure OpenAI API key in src/backend/GrantManagement.API/appsettings.json"
Write-Host "2. Run backend: .\scripts\run-backend.ps1"
Write-Host "3. Run frontend: .\scripts\run-frontend.ps1"
Write-Host ""
