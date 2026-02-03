# ============================================================================
# Script de correction automatique de la synchronisation
# ============================================================================
# Ce script corrige automatiquement les configurations pour synchroniser
# le backend et le frontend
#
# Usage : .\fix-sync.ps1
# ============================================================================

$ErrorActionPreference = "Stop"

# Couleurs
function Write-Success { param($msg) Write-Host "? $msg" -ForegroundColor Green }
function Write-Info { param($msg) Write-Host "??  $msg" -ForegroundColor Cyan }
function Write-Warning { param($msg) Write-Host "??  $msg" -ForegroundColor Yellow }

Write-Host ""
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Yellow
Write-Host "  ?? CORRECTION DE LA SYNCHRONISATION" -ForegroundColor White
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Yellow
Write-Host ""

# Configuration cible
$targetPort = 5153
$targetUrl = "http://localhost:$targetPort"

# ============================================================================
# ÉTAPE 1 : Corriger .env.local
# ============================================================================
Write-Info "Correction de la configuration frontend..."

$envPath = "finance-ui\.env.local"

if (Test-Path $envPath) {
    $envContent = Get-Content $envPath -Raw
    
    # Remplacer l'URL de l'API
    if ($envContent -match "NEXT_PUBLIC_API_URL=") {
        $newContent = $envContent -replace "NEXT_PUBLIC_API_URL=.*", "NEXT_PUBLIC_API_URL=$targetUrl"
    } else {
        # Ajouter la variable si elle n'existe pas
        $newContent = $envContent + "`nNEXT_PUBLIC_API_URL=$targetUrl`n"
    }
    
    Set-Content $envPath -Value $newContent -NoNewline
    Write-Success "Frontend : Configuration corrigée ($targetUrl)"
} else {
    Write-Warning "Fichier .env.local introuvable, création..."
    
    $envTemplate = @"
# Configuration de l'API Backend
NEXT_PUBLIC_API_URL=$targetUrl

# Configuration de l'API Gemini
NEXT_PUBLIC_GEMINI_API_KEY=AIzaSyCpYUPvjgvhPNtCjlJDg0ddmwCXPvUZRCg
"@
    
    Set-Content $envPath -Value $envTemplate
    Write-Success "Frontend : Fichier .env.local créé"
}

# ============================================================================
# ÉTAPE 2 : Vérifier launchSettings.json
# ============================================================================
Write-Info "Vérification de la configuration backend..."

$launchSettingsPath = "FinanceApp\Properties\launchSettings.json"
$launchSettings = Get-Content $launchSettingsPath -Raw | ConvertFrom-Json

$httpProfile = $launchSettings.profiles.http
if ($httpProfile.applicationUrl -eq $targetUrl) {
    Write-Success "Backend : Configuration correcte ($targetUrl)"
} else {
    Write-Warning "Backend : Port incorrect ($($httpProfile.applicationUrl))"
    Write-Host "   Le port devrait être $targetUrl" -ForegroundColor Gray
    Write-Host "   Modifiez manuellement FinanceApp\Properties\launchSettings.json" -ForegroundColor Gray
}

# ============================================================================
# ÉTAPE 3 : Vérifier api-config.ts
# ============================================================================
Write-Info "Vérification de la configuration API frontend..."

$apiConfigPath = "finance-ui\lib\api-config.ts"

if (Test-Path $apiConfigPath) {
    $apiConfig = Get-Content $apiConfigPath -Raw
    
    if ($apiConfig -match "process\.env\.NEXT_PUBLIC_API_URL") {
        Write-Success "API Config : Utilise correctement les variables d'environnement ?"
    } else {
        Write-Warning "API Config : Ne semble pas utiliser process.env.NEXT_PUBLIC_API_URL"
    }
} else {
    Write-Warning "Fichier api-config.ts introuvable"
}

# ============================================================================
# ÉTAPE 4 : Vérifier Program.cs (CORS)
# ============================================================================
Write-Info "Vérification de la configuration CORS..."

$programCsPath = "FinanceApp\Program.cs"
$programCs = Get-Content $programCsPath -Raw

if ($programCs -match 'WithOrigins\([^)]*"http://localhost:3000"') {
    Write-Success "CORS : localhost:3000 autorisé ?"
} else {
    Write-Warning "CORS : localhost:3000 non trouvé dans Program.cs"
    Write-Host "   Ajoutez manuellement dans Program.cs :" -ForegroundColor Gray
    Write-Host '   policy.WithOrigins("http://localhost:3000")' -ForegroundColor Gray
}

# ============================================================================
# RÉSUMÉ
# ============================================================================
Write-Host ""
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Yellow
Write-Host "  ? CORRECTIONS APPLIQUÉES" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Yellow
Write-Host ""
Write-Host "  ?? Configuration cible :" -ForegroundColor White
Write-Host "     Backend  : $targetUrl" -ForegroundColor Green
Write-Host "     Frontend : http://localhost:3000" -ForegroundColor Green
Write-Host ""
Write-Host "  ?? Prochaines étapes :" -ForegroundColor Cyan
Write-Host "     1. Vérifiez la configuration avec : .\check-sync.ps1" -ForegroundColor Gray
Write-Host "     2. Démarrez les services avec : .\start-both.ps1" -ForegroundColor Gray
Write-Host ""
Write-Host "???????????????????????????????????????????????????????????????" -ForegroundColor Yellow
Write-Host ""
