# 🚀 Script de déploiement rapide

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "   DEPLOIEMENT FINANCEAPP - VERCEL + RAILWAY" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Vérifier si Git est installé
$gitInstalled = Get-Command git -ErrorAction SilentlyContinue
if (-not $gitInstalled) {
    Write-Host "❌ Git n'est pas installé. Installez-le depuis https://git-scm.com" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Git est installé" -ForegroundColor Green
Write-Host ""

# Demander le nom d'utilisateur GitHub
Write-Host "📝 Configuration GitHub" -ForegroundColor Yellow
$githubUsername = Read-Host "Entrez votre nom d'utilisateur GitHub"
$repoName = Read-Host "Nom du repo (par défaut: FinanceApp)"
if ([string]::IsNullOrWhiteSpace($repoName)) {
    $repoName = "FinanceApp"
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "   ÉTAPE 1: INITIALISATION GIT" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# Vérifier si .git existe déjà
if (Test-Path ".git") {
    Write-Host "⚠️  Un repo Git existe déjà" -ForegroundColor Yellow
    $reinit = Read-Host "Voulez-vous réinitialiser? (o/N)"
    if ($reinit -eq "o" -or $reinit -eq "O") {
        Remove-Item -Recurse -Force .git
        git init
        Write-Host "✅ Repo Git réinitialisé" -ForegroundColor Green
    }
} else {
    git init
    Write-Host "✅ Repo Git initialisé" -ForegroundColor Green
}

# Ajouter tous les fichiers
Write-Host ""
Write-Host "📦 Ajout des fichiers..." -ForegroundColor Yellow
git add .
git commit -m "Initial commit - FinanceApp ready for Vercel + Railway deployment"
Write-Host "✅ Fichiers commités" -ForegroundColor Green

# Créer la branche main
git branch -M main

# Ajouter le remote
$remoteUrl = "https://github.com/$githubUsername/$repoName.git"
Write-Host ""
Write-Host "🔗 Configuration du remote: $remoteUrl" -ForegroundColor Yellow

$existingRemote = git remote get-url origin 2>$null
if ($existingRemote) {
    git remote remove origin
}
git remote add origin $remoteUrl

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "   PROCHAINES ÉTAPES MANUELLES" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1️⃣  CRÉER LE REPO SUR GITHUB:" -ForegroundColor Yellow
Write-Host "   → Allez sur https://github.com/new" -ForegroundColor White
Write-Host "   → Nom du repo: $repoName" -ForegroundColor White
Write-Host "   → Visibilité: Public ou Private" -ForegroundColor White
Write-Host "   → NE PAS ajouter de README, .gitignore, ou licence" -ForegroundColor White
Write-Host ""

Write-Host "2️⃣  PUSHER LE CODE:" -ForegroundColor Yellow
Write-Host "   Exécutez cette commande:" -ForegroundColor White
Write-Host "   git push -u origin main" -ForegroundColor Cyan
Write-Host ""

Write-Host "3️⃣  DEPLOYER LE BACKEND SUR RAILWAY:" -ForegroundColor Yellow
Write-Host "   → Allez sur https://railway.app" -ForegroundColor White
Write-Host "   → Cliquez sur 'New Project' → 'Deploy from GitHub repo'" -ForegroundColor White
Write-Host "   → Sélectionnez votre repo: $repoName" -ForegroundColor White
Write-Host "   → Railway détectera le Dockerfile automatiquement" -ForegroundColor White
Write-Host ""
Write-Host "   📊 Ajoutez PostgreSQL:" -ForegroundColor Cyan
Write-Host "   → Dans le projet, cliquez '+ New' → 'Database' → 'PostgreSQL'" -ForegroundColor White
Write-Host ""
Write-Host "   ⚙️  Variables d'environnement à ajouter:" -ForegroundColor Cyan
Write-Host "   ConnectionStrings__DefaultConnection = " -NoNewline -ForegroundColor White
Write-Host '${{Postgres.DATABASE_URL}}' -ForegroundColor Green
Write-Host "   JwtSettings__SecretKey = Y0uR_sUp3r_s3cr3t_jwt_k3y_2025_F1N@nc3@pp!" -ForegroundColor White
Write-Host "   JwtSettings__Issuer = FinanceApp" -ForegroundColor White
Write-Host "   JwtSettings__Audience = FinanceAppUsers" -ForegroundColor White
Write-Host "   ASPNETCORE_ENVIRONMENT = Production" -ForegroundColor White
Write-Host "   Groq__ApiKey = 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" -ForegroundColor White
Write-Host ""
Write-Host "   📝 Notez l'URL de votre API Railway (ex: https://financeapp-production.up.railway.app)" -ForegroundColor Magenta
Write-Host ""

Write-Host "4️⃣  DEPLOYER LE FRONTEND SUR VERCEL:" -ForegroundColor Yellow
Write-Host "   → Allez sur https://vercel.com" -ForegroundColor White
Write-Host "   → Cliquez 'Add New...' → 'Project'" -ForegroundColor White
Write-Host "   → Importez votre repo GitHub: $repoName" -ForegroundColor White
Write-Host "   → Root Directory: " -NoNewline -ForegroundColor White
Write-Host "finance-ui" -ForegroundColor Green
Write-Host ""
Write-Host "   ⚙️  Variables d'environnement à ajouter:" -ForegroundColor Cyan
Write-Host "   NEXT_PUBLIC_API_URL = [URL de votre API Railway]" -ForegroundColor White
Write-Host "   NEXT_PUBLIC_GEMINI_API_KEY = AIzaSyCpYUPvjgvhPNtCjlJDg0ddmwCXPvUZRCg" -ForegroundColor White
Write-Host ""

Write-Host "5️⃣  METTRE À JOUR LES CORS SUR RAILWAY:" -ForegroundColor Yellow
Write-Host "   Une fois Vercel déployé, notez l'URL (ex: https://finance-app-xyz.vercel.app)" -ForegroundColor White
Write-Host "   → Retournez sur Railway" -ForegroundColor White
Write-Host "   → Ajoutez cette variable d'environnement:" -ForegroundColor White
Write-Host "   AllowedOrigins__0 = https://votre-app.vercel.app" -ForegroundColor Green
Write-Host "   → Redéployez le backend Railway" -ForegroundColor White
Write-Host ""

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "   🎉 PRÊT POUR LE DÉPLOIEMENT!" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📖 Guide détaillé: DEPLOIEMENT-VERCEL-RAILWAY.md" -ForegroundColor Cyan
Write-Host ""
