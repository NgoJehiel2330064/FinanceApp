# ============================================================================
# Script de vérification de la synchronisation Backend/Frontend
# ============================================================================
# Ce script vérifie que le backend et le frontend sont configurés
# pour communiquer sur les mêmes ports
#
# Usage : .\check-sync.ps1
# ============================================================================

$ErrorActionPreference = "Continue"

# Couleurs
function Write-Success { param($msg) Write-Host "? $msg" -ForegroundColor Green }
function Write-Info { param($msg) Write-Host "??  $msg" -ForegroundColor Cyan }
function Write-Warning { param($msg) Write-Host "??  $msg" -ForegroundColor Yellow }
function Write-Error-Custom { param($msg) Write-Host "? $msg" -ForegroundColor Red }

Write-Host ""
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  ?? VÉRIFICATION DE LA SYNCHRONISATION" -ForegroundColor White
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# ============================================================================
# ÉTAPE 1 : Vérifier launchSettings.json
# ============================================================================
Write-Info "Vérification du backend (launchSettings.json)..."

$launchSettings = Get-Content "FinanceApp\Properties\launchSettings.json" -Raw | ConvertFrom-Json

$httpProfile = $launchSettings.profiles.http
if ($httpProfile) {
    $backendUrl = $httpProfile.applicationUrl
    Write-Host "   Backend configuré sur : $backendUrl" -ForegroundColor Gray
    
    if ($backendUrl -eq "http://localhost:5153") {
        Write-Success "Backend : Port 5153 ?"
    } else {
        Write-Error-Custom "Backend : Port incorrect ! (Attendu : 5153, Trouvé : $backendUrl)"
        $allGood = $false
    }
} else {
    Write-Error-Custom "Profile 'http' introuvable dans launchSettings.json"
    $allGood = $false
}

# ============================================================================
# ÉTAPE 2 : Vérifier .env.local
# ============================================================================
Write-Info "Vérification du frontend (.env.local)..."

if (Test-Path "finance-ui\.env.local") {
    $envContent = Get-Content "finance-ui\.env.local" -Raw
    
    if ($envContent -match "NEXT_PUBLIC_API_URL=(.*)") {
        $frontendUrl = $matches[1].Trim()
        Write-Host "   Frontend configuré sur : $frontendUrl" -ForegroundColor Gray
        
        if ($frontendUrl -eq "http://localhost:5153") {
            Write-Success "Frontend : URL API correcte ?"
        } else {
            Write-Error-Custom "Frontend : URL incorrecte ! (Attendu : http://localhost:5153, Trouvé : $frontendUrl)"
            $allGood = $false
        }
    } else {
        Write-Error-Custom "Variable NEXT_PUBLIC_API_URL introuvable"
        $allGood = $false
    }
} else {
    Write-Error-Custom "Fichier .env.local introuvable"
    $allGood = $false
}

# ============================================================================
# ÉTAPE 3 : Vérifier Program.cs (CORS)
# ============================================================================
Write-Info "Vérification de la configuration CORS..."

$programCs = Get-Content "FinanceApp\Program.cs" -Raw

if ($programCs -match 'WithOrigins\([^)]*"http://localhost:3000"') {
    Write-Success "CORS : localhost:3000 autorisé ?"
} else {
    Write-Warning "CORS : localhost:3000 non trouvé dans Program.cs"
    $allGood = $false
}

# ============================================================================
# ÉTAPE 4 : Vérifier les services en cours
# ============================================================================
Write-Info "Vérification des services en cours d'exécution..."

# Backend
$backendRunning = Get-NetTCPConnection -LocalPort 5153 -ErrorAction SilentlyContinue
if ($backendRunning) {
    Write-Success "Backend : En cours d'exécution sur le port 5153 ?"
} else {
    Write-Warning "Backend : Pas de processus sur le port 5153"
    Write-Host "   Démarrez le backend avec : .\start-both.ps1" -ForegroundColor Gray
}

# Frontend
$frontendRunning = Get-NetTCPConnection -LocalPort 3000 -ErrorAction SilentlyContinue
if ($frontendRunning) {
    Write-Success "Frontend : En cours d'exécution sur le port 3000 ?"
} else {
    Write-Warning "Frontend : Pas de processus sur le port 3000"
    Write-Host "   Démarrez le frontend avec : .\start-both.ps1" -ForegroundColor Gray
}

# PostgreSQL
$dockerRunning = docker ps --filter "name=postgres" --format "{{.Names}}" 2>$null
if ($dockerRunning) {
    Write-Success "PostgreSQL : En cours d'exécution ?"
} else {
    Write-Warning "PostgreSQL : Pas de conteneur en cours d'exécution"
    Write-Host "   Démarrez PostgreSQL avec : docker-compose up -d" -ForegroundColor Gray
}

# ============================================================================
# ÉTAPE 5 : Test de connectivité
# ============================================================================
if ($backendRunning) {
    Write-Info "Test de connectivité API..."
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5153/api/transactions" -Method GET -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Success "API : Répond correctement ?"
        } else {
            Write-Warning "API : Code HTTP $($response.StatusCode)"
        }
    } catch {
        Write-Warning "API : Pas de réponse (normal si aucune transaction)"
    }
}

# ============================================================================
# RÉSUMÉ
# ============================================================================
Write-Host ""
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan

if ($allGood) {
    Write-Host "  ? CONFIGURATION PARFAITEMENT SYNCHRONISÉE" -ForegroundColor Green
} else {
    Write-Host "  ??  PROBLÈMES DÉTECTÉS" -ForegroundColor Yellow
}

Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# RECOMMANDATIONS
# ============================================================================
if (-not $allGood) {
    Write-Host "  ?? ACTIONS RECOMMANDÉES :" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  1. Corriger la configuration avec :" -ForegroundColor White
    Write-Host "     .\fix-sync.ps1" -ForegroundColor Green
    Write-Host ""
    Write-Host "  2. Redémarrer les services avec :" -ForegroundColor White
    Write-Host "     .\start-both.ps1" -ForegroundColor Green
    Write-Host ""
}

Write-Host "  ?? Pour démarrer tous les services :" -ForegroundColor Cyan
Write-Host "     .\start-both.ps1" -ForegroundColor Green
Write-Host ""
