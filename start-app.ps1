# ============================================================================
# Script de démarrage automatique - FinanceApp
# ============================================================================
# Ce script démarre automatiquement tous les services nécessaires :
# 1. PostgreSQL (Docker)
# 2. API ASP.NET Core
# 
# Usage : .\start-app.ps1

Write-Host "?? Démarrage de FinanceApp..." -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# ÉTAPE 1 : Vérifier Docker
# ============================================================================
Write-Host "?? Vérification de Docker..." -ForegroundColor Yellow

try {
    $dockerVersion = docker --version
    Write-Host "? Docker détecté : $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "? Docker n'est pas installé ou n'est pas démarré !" -ForegroundColor Red
    Write-Host "   Veuillez installer Docker Desktop : https://www.docker.com/products/docker-desktop/" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# ============================================================================
# ÉTAPE 2 : Démarrer PostgreSQL
# ============================================================================
Write-Host "?? Démarrage de PostgreSQL..." -ForegroundColor Yellow

# Vérifier si le conteneur existe déjà
$postgresContainer = docker ps -a --filter "name=postgres_db" --format "{{.Names}}"

if ($postgresContainer) {
    Write-Host "   Conteneur PostgreSQL trouvé" -ForegroundColor Gray
    
    # Vérifier s'il tourne déjà
    $isRunning = docker ps --filter "name=postgres_db" --format "{{.Names}}"
    
    if ($isRunning) {
        Write-Host "? PostgreSQL est déjà en cours d'exécution" -ForegroundColor Green
    } else {
        Write-Host "   Démarrage du conteneur existant..." -ForegroundColor Gray
        docker start postgres_db | Out-Null
        Start-Sleep -Seconds 2
        Write-Host "? PostgreSQL démarré" -ForegroundColor Green
    }
} else {
    Write-Host "   Création du conteneur PostgreSQL..." -ForegroundColor Gray
    docker-compose up -d
    Start-Sleep -Seconds 5
    Write-Host "? PostgreSQL créé et démarré" -ForegroundColor Green
}

Write-Host ""

# ============================================================================
# ÉTAPE 3 : Vérifier la connexion PostgreSQL
# ============================================================================
Write-Host "?? Vérification de la connexion PostgreSQL..." -ForegroundColor Yellow

$maxRetries = 5
$retryCount = 0
$connected = $false

while (-not $connected -and $retryCount -lt $maxRetries) {
    try {
        $testConnection = docker exec postgres_db pg_isready -U postgres 2>&1
        if ($testConnection -like "*accepting connections*") {
            $connected = $true
            Write-Host "? PostgreSQL accepte les connexions" -ForegroundColor Green
        } else {
            throw "Not ready"
        }
    } catch {
        $retryCount++
        Write-Host "   Tentative $retryCount/$maxRetries..." -ForegroundColor Gray
        Start-Sleep -Seconds 2
    }
}

if (-not $connected) {
    Write-Host "??  PostgreSQL ne répond pas après $maxRetries tentatives" -ForegroundColor Yellow
    Write-Host "   L'API pourrait avoir des problèmes de connexion" -ForegroundColor Yellow
}

Write-Host ""

# ============================================================================
# ÉTAPE 4 : Vérifier le port 5152
# ============================================================================
Write-Host "?? Vérification du port 5152..." -ForegroundColor Yellow

$portInUse = netstat -ano | Select-String ":5152" | Select-Object -First 1

if ($portInUse) {
    Write-Host "??  Le port 5152 est déjà utilisé !" -ForegroundColor Yellow
    
    # Extraire le PID
    $pidMatch = $portInUse -match '\s+(\d+)\s*$'
    if ($pidMatch) {
        $pid = $Matches[1]
        Write-Host "   Processus : PID $pid" -ForegroundColor Gray
        
        # Demander confirmation pour tuer le processus
        $response = Read-Host "   Voulez-vous terminer ce processus ? (O/N)"
        
        if ($response -eq "O" -or $response -eq "o") {
            try {
                taskkill /F /PID $pid | Out-Null
                Write-Host "? Processus terminé" -ForegroundColor Green
                Start-Sleep -Seconds 1
            } catch {
                Write-Host "? Impossible de terminer le processus" -ForegroundColor Red
                Write-Host "   Exécutez : taskkill /F /PID $pid" -ForegroundColor Yellow
                exit 1
            }
        } else {
            Write-Host "? Impossible de démarrer l'API sur le port 5152" -ForegroundColor Red
            exit 1
        }
    }
} else {
    Write-Host "? Port 5152 disponible" -ForegroundColor Green
}

Write-Host ""

# ============================================================================
# ÉTAPE 5 : Vérifier User Secrets
# ============================================================================
Write-Host "?? Vérification des User Secrets..." -ForegroundColor Yellow

Push-Location FinanceApp

try {
    $secrets = dotnet user-secrets list 2>&1
    
    if ($secrets -like "*Gemini:ApiKey*") {
        Write-Host "? Clé API Gemini configurée" -ForegroundColor Green
    } else {
        Write-Host "??  Clé API Gemini non configurée" -ForegroundColor Yellow
        Write-Host "   Les conseils IA ne fonctionneront pas" -ForegroundColor Yellow
        Write-Host "   Pour configurer : dotnet user-secrets set 'Gemini:ApiKey' 'VOTRE_CLE'" -ForegroundColor Gray
    }
} catch {
    Write-Host "??  Impossible de vérifier les User Secrets" -ForegroundColor Yellow
}

Pop-Location

Write-Host ""

# ============================================================================
# ÉTAPE 6 : Appliquer les migrations EF Core
# ============================================================================
Write-Host "?? Vérification de la base de données..." -ForegroundColor Yellow

Push-Location FinanceApp

try {
    Write-Host "   Application des migrations..." -ForegroundColor Gray
    $migrationOutput = dotnet ef database update 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Base de données à jour" -ForegroundColor Green
    } else {
        Write-Host "??  Problème avec les migrations" -ForegroundColor Yellow
        Write-Host "   L'API démarrera quand même" -ForegroundColor Gray
    }
} catch {
    Write-Host "??  Impossible d'appliquer les migrations" -ForegroundColor Yellow
}

Pop-Location

Write-Host ""

# ============================================================================
# ÉTAPE 7 : Démarrer l'API ASP.NET Core
# ============================================================================
Write-Host "?? Démarrage de l'API ASP.NET Core..." -ForegroundColor Yellow

Push-Location FinanceApp

Write-Host ""
Write-Host "??????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "? FinanceApp est prêt !" -ForegroundColor Green
Write-Host ""
Write-Host "   ?? API HTTP   : http://localhost:5152" -ForegroundColor White
Write-Host "   ?? API HTTPS  : https://localhost:7219" -ForegroundColor White
Write-Host "   ?? Swagger    : http://localhost:5152/swagger" -ForegroundColor White
Write-Host "   ?? PostgreSQL : localhost:5432" -ForegroundColor White
Write-Host ""
Write-Host "   ?? Endpoints disponibles :" -ForegroundColor White
Write-Host "      • GET  /api/transactions" -ForegroundColor Gray
Write-Host "      • POST /api/transactions" -ForegroundColor Gray
Write-Host "      • GET  /api/finance/advice" -ForegroundColor Gray
Write-Host ""
Write-Host "   ?? Pour arrêter : Ctrl+C" -ForegroundColor White
Write-Host "??????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Démarrer l'API (bloquant)
dotnet run

Pop-Location
