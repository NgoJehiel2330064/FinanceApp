# ============================================================================
# Script d'arrêt - FinanceApp
# ============================================================================
# Ce script arrête proprement tous les services
# Usage : .\stop-app.ps1

Write-Host "?? Arrêt de FinanceApp..." -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# ÉTAPE 1 : Arrêter l'API ASP.NET Core
# ============================================================================
Write-Host "?? Arrêt de l'API ASP.NET Core..." -ForegroundColor Yellow

$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue

if ($dotnetProcesses) {
    foreach ($process in $dotnetProcesses) {
        try {
            # Vérifier si c'est notre application
            $processPath = $process.Path
            if ($processPath -like "*FinanceApp*") {
                Stop-Process -Id $process.Id -Force
                Write-Host "   ? Processus dotnet arrêté (PID: $($process.Id))" -ForegroundColor Green
            }
        } catch {
            Write-Host "   ??  Impossible d'arrêter le processus $($process.Id)" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "   ??  Aucun processus dotnet en cours d'exécution" -ForegroundColor Gray
}

Write-Host ""

# ============================================================================
# ÉTAPE 2 : Libérer le port 5152
# ============================================================================
Write-Host "?? Vérification du port 5152..." -ForegroundColor Yellow

$portInUse = netstat -ano | Select-String ":5152" | Select-Object -First 1

if ($portInUse) {
    $pidMatch = $portInUse -match '\s+(\d+)\s*$'
    if ($pidMatch) {
        $pid = $Matches[1]
        try {
            taskkill /F /PID $pid 2>&1 | Out-Null
            Write-Host "   ? Port 5152 libéré" -ForegroundColor Green
        } catch {
            Write-Host "   ??  Impossible de libérer le port 5152" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "   ? Port 5152 déjà libre" -ForegroundColor Green
}

Write-Host ""

# ============================================================================
# ÉTAPE 3 : Arrêter PostgreSQL (optionnel)
# ============================================================================
Write-Host "?? Gestion de PostgreSQL..." -ForegroundColor Yellow

$response = Read-Host "   Voulez-vous arrêter PostgreSQL ? (O/N)"

if ($response -eq "O" -or $response -eq "o") {
    try {
        docker-compose down
        Write-Host "   ? PostgreSQL arrêté" -ForegroundColor Green
    } catch {
        Write-Host "   ??  Impossible d'arrêter PostgreSQL" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ??  PostgreSQL reste en cours d'exécution" -ForegroundColor Gray
    Write-Host "   ?? Pour l'arrêter manuellement : docker-compose down" -ForegroundColor Gray
}

Write-Host ""
Write-Host "? Arrêt terminé !" -ForegroundColor Green
Write-Host ""
