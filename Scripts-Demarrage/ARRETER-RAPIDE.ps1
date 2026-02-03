# ============================================================================
# SCRIPT D'ARRET RAPIDE - FinanceApp
# ============================================================================
# Arrete le Backend et Frontend en 1 clique
#
# Usage: .\ARRETER-RAPIDE.ps1
# ============================================================================

$ErrorActionPreference = "Continue"

# Couleurs
function Write-Success { param($msg) Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Info { param($msg) Write-Host "[i] $msg" -ForegroundColor Cyan }
function Write-Warning { param($msg) Write-Host "[!] $msg" -ForegroundColor Yellow }

Clear-Host
Write-Host ""
Write-Host "========================================" -ForegroundColor Red
Write-Host "  ARRET FINANCEAPP" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Red
Write-Host ""

# ============================================================================
# Frontend (port 3000)
# ============================================================================
Write-Info "Arret du Frontend (Node.js)..."
$frontendProc = Get-Process -Name "node" -ErrorAction SilentlyContinue
if ($frontendProc) {
    Stop-Process -Name "node" -Force -ErrorAction SilentlyContinue
    Write-Success "Frontend arrete"
} else {
    Write-Warning "Frontend non actif"
}

# ============================================================================
# Backend (port 5153)
# ============================================================================
Write-Info "Arret du Backend (.NET)..."
$backendProc = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | 
    Where-Object { $_.MainModule.FileName -like "*FinanceApp*" }
if ($backendProc) {
    Stop-Process -InputObject $backendProc -Force -ErrorAction SilentlyContinue
    Write-Success "Backend arrete"
} else {
    Write-Warning "Backend non actif"
}

# ============================================================================
# Nettoyage des shells PowerShell
# ============================================================================
Write-Info "Nettoyage des terminals..."
Get-Process powershell -ErrorAction SilentlyContinue | 
    Where-Object { $_.MainWindowTitle -match "BACKEND|FRONTEND" } | 
    Stop-Process -Force -ErrorAction SilentlyContinue

Write-Success "Terminals fermes"

# ============================================================================
# Resume
# ============================================================================
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  TOUS LES SERVICES ARRETES" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "[OK] Frontend arrete (port 3000)" -ForegroundColor Green
Write-Host "[OK] Backend arrete (port 5153)" -ForegroundColor Green
Write-Host ""
Write-Host "Pour redemarrer :" -ForegroundColor Yellow
Write-Host "  .\DEMARRER-RAPIDE.ps1" -ForegroundColor Green
Write-Host ""

Start-Sleep -Seconds 2
