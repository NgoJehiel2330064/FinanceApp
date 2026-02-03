# ============================================================================
# Script d'Installation des Raccourcis Bureau
# ============================================================================
# Ce script cree des raccourcis sur votre bureau Windows pour
# demarrer/arreter FinanceApp d'un simple double-clic
# ============================================================================

$ErrorActionPreference = "Continue"

# Couleurs
function Write-Success { param($msg) Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Info { param($msg) Write-Host "[i] $msg" -ForegroundColor Cyan }
function Write-Warning { param($msg) Write-Host "[!] $msg" -ForegroundColor Yellow }

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  INSTALLATION DES RACCOURCIS BUREAU" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Chemins
$projectPath = Split-Path -Parent $PSScriptRoot
$scriptsPath = $PSScriptRoot
$desktopPath = [Environment]::GetFolderPath("Desktop")

Write-Info "Dossier du projet : $projectPath"
Write-Info "Dossier des scripts : $scriptsPath"
Write-Info "Bureau : $desktopPath"
Write-Host ""

# Creer les raccourcis sur le bureau
Write-Info "Creation des raccourcis sur le bureau..."

$shell = New-Object -ComObject WScript.Shell

$shortcuts = @(
    @{
        Name = "[START] FinanceApp.lnk"
        Script = "DEMARRER-RAPIDE.ps1"
        Icon = "imageres.dll,1"
        Description = "Demarrer backend + frontend"
    },
    @{
        Name = "[STOP] FinanceApp.lnk"
        Script = "ARRETER-RAPIDE.ps1"
        Icon = "imageres.dll,84"
        Description = "Arreter tous les services"
    }
)

foreach ($shortcut in $shortcuts) {
    $shortcutPath = Join-Path $desktopPath $shortcut.Name
    
    if (Test-Path $shortcutPath) {
        Remove-Item $shortcutPath -Force
        Write-Warning "Raccourci existant supprime : $($shortcut.Name)"
    }
    
    $scriptPath = Join-Path $scriptsPath $shortcut.Script
    
    $link = $shell.CreateShortcut($shortcutPath)
    $link.TargetPath = "powershell.exe"
    $link.Arguments = "-ExecutionPolicy Bypass -NoProfile -File `"$scriptPath`""
    $link.WorkingDirectory = $projectPath
    $link.Description = $shortcut.Description
    $link.IconLocation = "C:\Windows\System32\$($shortcut.Icon)"
    $link.Save()
    
    Write-Success "Raccourci cree : $($shortcut.Name)"
}

Write-Host ""

# Resume
Write-Host "========================================" -ForegroundColor Green
Write-Host "  INSTALLATION TERMINEE !" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "2 raccourcis ont ete crees sur votre bureau :" -ForegroundColor White
Write-Host ""
Write-Host "  [START] FinanceApp" -ForegroundColor Green
Write-Host "          Double-cliquez pour demarrer" -ForegroundColor Gray
Write-Host ""
Write-Host "  [STOP] FinanceApp" -ForegroundColor Red
Write-Host "         Double-cliquez pour arreter" -ForegroundColor Gray
Write-Host ""
Write-Host "Scripts dans: $scriptsPath" -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
