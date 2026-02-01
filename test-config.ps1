# ============================================================================
# Script de test rapide - FinanceApp
# ============================================================================
# Vérifie que tout est configuré correctement
# Usage : .\test-config.ps1

Write-Host "?? Vérification de la configuration de FinanceApp..." -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# ============================================================================
# Test 1 : Docker
# ============================================================================
Write-Host "1??  Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Docker est installé : $dockerVersion" -ForegroundColor Green
    } else {
        Write-Host "   ? Docker n'est pas installé" -ForegroundColor Red
        $allGood = $false
    }
} catch {
    Write-Host "   ? Docker n'est pas installé ou n'est pas démarré" -ForegroundColor Red
    $allGood = $false
}
Write-Host ""

# ============================================================================
# Test 2 : PostgreSQL
# ============================================================================
Write-Host "2??  PostgreSQL (Docker)..." -ForegroundColor Yellow
try {
    $postgresContainer = docker ps --filter "name=postgres_db" --format "{{.Names}}" 2>&1
    if ($postgresContainer -eq "postgres_db") {
        Write-Host "   ? PostgreSQL est en cours d'exécution" -ForegroundColor Green
        
        # Test de connexion
        $testConnection = docker exec postgres_db pg_isready -U postgres 2>&1
        if ($testConnection -like "*accepting connections*") {
            Write-Host "   ? PostgreSQL accepte les connexions" -ForegroundColor Green
        } else {
            Write-Host "   ??  PostgreSQL ne répond pas encore" -ForegroundColor Yellow
        }
    } else {
        Write-Host "   ? PostgreSQL n'est pas démarré" -ForegroundColor Red
        Write-Host "   ?? Lancez : docker-compose up -d" -ForegroundColor Gray
        $allGood = $false
    }
} catch {
    Write-Host "   ? Erreur lors de la vérification de PostgreSQL" -ForegroundColor Red
    $allGood = $false
}
Write-Host ""

# ============================================================================
# Test 3 : Port 5152
# ============================================================================
Write-Host "3??  Port 5152..." -ForegroundColor Yellow
$portInUse = netstat -ano | Select-String ":5152" | Select-Object -First 1
if ($portInUse) {
    $pidMatch = $portInUse -match '\s+(\d+)\s*$'
    if ($pidMatch) {
        $pid = $Matches[1]
        Write-Host "   ??  Port 5152 est utilisé par le processus PID $pid" -ForegroundColor Yellow
        Write-Host "   ?? Si c'est votre API, c'est normal. Sinon : taskkill /F /PID $pid" -ForegroundColor Gray
    }
} else {
    Write-Host "   ? Port 5152 est libre" -ForegroundColor Green
}
Write-Host ""

# ============================================================================
# Test 4 : .NET SDK
# ============================================================================
Write-Host "4??  .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? .NET SDK installé : version $dotnetVersion" -ForegroundColor Green
    } else {
        Write-Host "   ? .NET SDK n'est pas installé" -ForegroundColor Red
        $allGood = $false
    }
} catch {
    Write-Host "   ? .NET SDK n'est pas installé" -ForegroundColor Red
    $allGood = $false
}
Write-Host ""

# ============================================================================
# Test 5 : User Secrets (Clé API Gemini)
# ============================================================================
Write-Host "5??  User Secrets (Clé API Gemini)..." -ForegroundColor Yellow
Push-Location FinanceApp
try {
    $secrets = dotnet user-secrets list 2>&1
    
    if ($secrets -like "*Gemini:ApiKey = AIzaSy*") {
        Write-Host "   ? Clé API Gemini configurée (commence par AIzaSy...)" -ForegroundColor Green
    } elseif ($secrets -like "*Gemini:ApiKey*") {
        Write-Host "   ??  Clé API trouvée mais format inhabituel" -ForegroundColor Yellow
    } else {
        Write-Host "   ? Clé API Gemini NON configurée" -ForegroundColor Red
        Write-Host "   ?? Lancez : dotnet user-secrets set 'Gemini:ApiKey' 'VOTRE_CLE'" -ForegroundColor Gray
        $allGood = $false
    }
} catch {
    Write-Host "   ? Impossible de vérifier les User Secrets" -ForegroundColor Red
    $allGood = $false
}
Pop-Location
Write-Host ""

# ============================================================================
# Test 6 : Base de données
# ============================================================================
Write-Host "6??  Base de données..." -ForegroundColor Yellow
Push-Location FinanceApp
try {
    $dbCheck = dotnet ef database update --dry-run 2>&1
    if ($dbCheck -like "*No pending model changes*" -or $dbCheck -like "*already applied*") {
        Write-Host "   ? Base de données à jour" -ForegroundColor Green
    } else {
        Write-Host "   ??  Migrations en attente" -ForegroundColor Yellow
        Write-Host "   ?? Lancez : dotnet ef database update" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ??  Impossible de vérifier la base de données" -ForegroundColor Yellow
    Write-Host "   ?? Vérifiez que PostgreSQL est démarré" -ForegroundColor Gray
}
Pop-Location
Write-Host ""

# ============================================================================
# Test 7 : Fichiers de configuration
# ============================================================================
Write-Host "7??  Fichiers de configuration..." -ForegroundColor Yellow
$configFiles = @(
    "FinanceApp\appsettings.json",
    "FinanceApp\appsettings.Development.json",
    "docker-compose.yml",
    ".gitignore"
)

$missingFiles = @()
foreach ($file in $configFiles) {
    if (Test-Path $file) {
        Write-Host "   ? $file" -ForegroundColor Green
    } else {
        Write-Host "   ? $file manquant" -ForegroundColor Red
        $missingFiles += $file
        $allGood = $false
    }
}
Write-Host ""

# ============================================================================
# Test 8 : Documentation
# ============================================================================
Write-Host "8??  Documentation..." -ForegroundColor Yellow
$docFiles = @(
    "README.md",
    "SECRETS-CONFIGURATION.md",
    "TROUBLESHOOTING.md",
    "FRONTEND-CONFIGURATION.md"
)

foreach ($file in $docFiles) {
    if (Test-Path $file) {
        Write-Host "   ? $file" -ForegroundColor Green
    } else {
        Write-Host "   ??  $file manquant (optionnel)" -ForegroundColor Yellow
    }
}
Write-Host ""

# ============================================================================
# Résumé
# ============================================================================
Write-Host "??????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
if ($allGood) {
    Write-Host "? Configuration complète ! Vous êtes prêt à démarrer." -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Pour démarrer l'application :" -ForegroundColor White
    Write-Host "   .\start-app.ps1" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "?? Ou manuellement :" -ForegroundColor White
    Write-Host "   1. docker-compose up -d" -ForegroundColor Gray
    Write-Host "   2. cd FinanceApp" -ForegroundColor Gray
    Write-Host "   3. dotnet run" -ForegroundColor Gray
} else {
    Write-Host "??  Certains éléments nécessitent votre attention." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "?? Consultez la documentation :" -ForegroundColor White
    Write-Host "   • README.md - Guide de démarrage" -ForegroundColor Gray
    Write-Host "   • TROUBLESHOOTING.md - Résolution de problèmes" -ForegroundColor Gray
}
Write-Host "??????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""
