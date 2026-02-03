# ============================================================================
# SCRIPT DE DEMARRAGE RAPIDE - FinanceApp
# ============================================================================
# Lance facilement le Backend et Frontend
# 
# Usage: .\DEMARRER-RAPIDE.ps1
# ============================================================================

$ErrorActionPreference = "Stop"

# Couleurs
function Write-Success { param($msg) Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Info { param($msg) Write-Host "[i] $msg" -ForegroundColor Cyan }
function Write-Warning { param($msg) Write-Host "[!] $msg" -ForegroundColor Yellow }
function Write-Error-Msg { param($msg) Write-Host "[X] $msg" -ForegroundColor Red }

Clear-Host
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  DEMARRAGE FINANCEAPP" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$rootPath = Split-Path -Parent $PSScriptRoot

# ============================================================================
# 1. Verifier les dossiers
# ============================================================================
Write-Info "Verification des dossiers..."

$backendPath = Join-Path $rootPath "FinanceApp"
$frontendPath = Join-Path $rootPath "finance-ui"

if (-not (Test-Path $backendPath)) {
    Write-Error-Msg "Dossier backend introuvable: $backendPath"
    exit 1
}
Write-Success "Backend trouve"

if (-not (Test-Path $frontendPath)) {
    Write-Error-Msg "Dossier frontend introuvable: $frontendPath"
    exit 1
}
Write-Success "Frontend trouve"

# ============================================================================
# 2. Arreter les anciens processus
# ============================================================================
Write-Info "Arret des anciens services..."

Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | 
    Where-Object { $_.MainModule.FileName -like "*FinanceApp*" } | 
    Stop-Process -Force -ErrorAction SilentlyContinue

Get-Process -Name "node" -ErrorAction SilentlyContinue | 
    Stop-Process -Force -ErrorAction SilentlyContinue

Start-Sleep -Seconds 2
Write-Success "Services arretes"

# ============================================================================
# 3. Demarrer Backend
# ============================================================================
Write-Info "Demarrage du Backend (.NET)..."
Write-Host "  URL: http://localhost:5153" -ForegroundColor Yellow
Write-Host "  Swagger: http://localhost:5153/swagger" -ForegroundColor Yellow

Push-Location $backendPath
Start-Process powershell -ArgumentList `
    "-NoExit",
    "-NoProfile",
    "-Command",
    "Write-Host 'BACKEND EN COURS DE DEMARRAGE...' -ForegroundColor Green; cd `"$backendPath`"; dotnet run --launch-profile http"
Pop-Location

Write-Success "Backend lance"
Start-Sleep -Seconds 5

# ============================================================================
# 4. Demarrer Frontend
# ============================================================================
Write-Info "Demarrage du Frontend (Next.js)..."
Write-Host "  URL: http://localhost:3000" -ForegroundColor Yellow

Push-Location $frontendPath
Start-Process powershell -ArgumentList `
    "-NoExit",
    "-NoProfile",
    "-Command",
    "Write-Host 'FRONTEND EN COURS DE DEMARRAGE...' -ForegroundColor Green; cd `"$frontendPath`"; npm run dev"
Pop-Location

Write-Success "Frontend lance"

# ============================================================================
# 5. Resume et ouverture du navigateur
# ============================================================================
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  FINANCEAPP EST PRET" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Frontend  : http://localhost:3000" -ForegroundColor Cyan
Write-Host "Backend   : http://localhost:5153" -ForegroundColor Cyan
Write-Host "Swagger   : http://localhost:5153/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Patientez 10-15 secondes pour le demarrage complet..." -ForegroundColor Yellow
Write-Host ""

Start-Sleep -Seconds 10
Start-Process "http://localhost:3000"

Write-Host "Application ouverte dans votre navigateur" -ForegroundColor Green
Write-Host ""
