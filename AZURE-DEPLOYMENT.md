# Déploiement Azure (App Service) — FinanceApp

Ce guide part du principe que :
- Le backend est une app ASP.NET Core (.NET 8) dans `FinanceApp/`
- Le frontend est un Next.js dans `finance-ui/`

## 1) Backend (API) sur Azure App Service (Code)

### A. Créer l’App Service
Dans Azure Portal → **App Services** → **Créer** → **Application web** :
- Publier : **Code**
- Pile d’exécution : **.NET 8 (LTS)**
- Système : **Linux**
- Plan : au minimum **B1** recommandé (le plan Gratuit peut fonctionner mais est très limité)

Après création, note l’URL :
- `https://<nom-app>.azurewebsites.net`

### B. Variables d’environnement (obligatoire)
Azure Portal → App Service → **Configuration** → **Paramètres d’application** :

```
ConnectionStrings__DefaultConnection = <CHANGE_ME_POSTGRES_CONNECTION_STRING>
Jwt__Key = <CHANGE_ME_LONG_RANDOM_SECRET>
Jwt__Issuer = FinanceApp
Jwt__Audience = FinanceAppUsers
Jwt__ExpiresMinutes = 60
ASPNETCORE_ENVIRONMENT = Production
Groq__ApiKey = <CHANGE_ME>
Groq__Model = mixtral-8x7b-32768
Groq__BaseUrl = https://api.groq.com/openai/v1
Groq__Temperature = 0.3
Groq__MaxTokens = 150
AllowedOrigins__0 = <CHANGE_ME_FRONTEND_URL>
```

### C. Base de données PostgreSQL
Crée une base **Azure Database for PostgreSQL – Flexible Server**.

Récupère une chaîne de connexion (format Npgsql) du style :

```
Host=<server>.postgres.database.azure.com;Port=5432;Database=<db>;Username=<user>;Password=<pwd>;Ssl Mode=Require;Trust Server Certificate=true
```

Puis colle-la dans :
- `ConnectionStrings__DefaultConnection`

> Important : côté PostgreSQL, pense à autoriser l’accès réseau (Public access + règles firewall) pour que l’App Service puisse se connecter.

### D. Déploiement depuis GitHub (automatique)
Le repo contient un workflow prêt :
- `.github/workflows/azure-financeapp-api.yml`

Pour l’activer :
1. Azure Portal → App Service (backend) → **Get publish profile**
2. GitHub → Repo → **Settings → Secrets and variables → Actions**
3. Ajoute un secret :
   - Nom : `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Valeur : contenu du publish profile
4. Push sur la branche `azure-deploy` → déploiement automatique

### E. Test
Teste :
- `https://<backend>/health`

## 2) Frontend (Next.js)

Deux options simples :

### Option A (recommandée) : Vercel
- Déploie `finance-ui` sur Vercel
- Configure `NEXT_PUBLIC_API_URL = https://<backend>`

### Option B : Azure App Service (Node)
Créer une 2e App Service Linux (Node 20) puis :
- Build : `npm ci && npm run build`
- Start : `npm run start`
- App settings :
  - `NEXT_PUBLIC_API_URL = https://<backend>`

## 3) CORS
Dans le backend, mets :
- `AllowedOrigins__0 = https://<frontend>`

Puis redémarre l’App Service (ou attends le redémarrage auto).

