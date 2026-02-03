# Guide de déploiement Vercel + Railway

## Étape 1 : Préparation du code (déjà fait)

Les fichiers nécessaires sont déjà créés :
- `Dockerfile` pour le backend
- `vercel.json` pour le frontend
- `.gitignore` et `.dockerignore`

## Étape 2 : Créer un repo GitHub

1. Allez sur https://github.com et créez un nouveau repo "FinanceApp"
2. Dans votre terminal :

```powershell
cd C:\Users\GOAT\OneDrive\Documents\FinanceApp
git init
git add .
git commit -m "Initial commit - FinanceApp ready for deployment"
git branch -M main
git remote add origin https://github.com/VOTRE_USERNAME/FinanceApp.git
git push -u origin main
```

## Étape 3 : Déployer le Backend sur Railway

1. Allez sur https://railway.app
2. Cliquez sur **"New Project"**
3. Sélectionnez **"Deploy from GitHub repo"**
4. Connectez votre compte GitHub et sélectionnez le repo "FinanceApp"
5. Railway détectera automatiquement le Dockerfile

### Configuration Railway

**A. Ajouter PostgreSQL**
1. Dans votre projet Railway, cliquez sur **"+ New"**
2. Sélectionnez **"Database" → "Add PostgreSQL"**
3. Railway créera automatiquement la variable `DATABASE_URL`

**B. Variables d'environnement (Railway)**
Allez dans l'onglet **"Variables"** et ajoutez :

```
ConnectionStrings__DefaultConnection = ${Postgres.DATABASE_URL}
Jwt__Key = <CHANGE_ME>
Jwt__Issuer = FinanceApp
Jwt__Audience = FinanceAppUsers
Jwt__ExpiresMinutes = 60
ASPNETCORE_ENVIRONMENT = Production
Groq__ApiKey = <CHANGE_ME>
Groq__Model = mixtral-8x7b-32768
Groq__BaseUrl = https://api.groq.com/openai/v1
Groq__Temperature = 0.3
Groq__MaxTokens = 150
```

**C. Déployer**
1. Cliquez sur **"Deploy"**
2. Attendez quelques minutes
3. Railway vous donnera une URL (ex: `https://financeapp-production.up.railway.app`)

**D. Tester**
Ouvrez : `https://votre-app-railway.up.railway.app/health`

## Étape 4 : Déployer le Frontend sur Vercel

1. Allez sur https://vercel.com
2. Cliquez sur **"Add New... → Project"**
3. Importez votre repo GitHub "FinanceApp"
4. Vercel détectera automatiquement Next.js

### Configuration Vercel

**A. Configuration du projet**
- Framework Preset : **Next.js**
- Root Directory : **finance-ui**
- Build Command : `npm run build`
- Output Directory : `.next`

**B. Variables d'environnement (Vercel)**

```
NEXT_PUBLIC_API_URL = https://votre-app-railway.up.railway.app
```

**C. Déployer**
1. Cliquez sur **"Deploy"**
2. Attendez 2-3 minutes
3. Vercel vous donnera une URL (ex: `https://finance-app-xyz.vercel.app`)

## Étape 5 : Configuration CORS du Backend

Le backend doit autoriser le domaine Vercel :

1. Notez votre URL Vercel (ex: `https://finance-app-xyz.vercel.app`)
2. Dans Railway, ajoutez cette variable d'environnement :
```
AllowedOrigins__0 = https://finance-app-xyz.vercel.app
```

## Étape 6 : Tester

1. Ouvrez votre URL Vercel dans n'importe quel navigateur/mobile
2. Créez un compte
3. Testez toutes les fonctionnalités
4. Partagez l'URL avec vos amis

## En cas de problème

### Backend ne démarre pas sur Railway
- Vérifiez les logs dans Railway
- Assurez-vous que PostgreSQL est bien connecté
- Vérifiez que toutes les variables d'environnement sont définies

### Frontend ne charge pas
- Vérifiez que `NEXT_PUBLIC_API_URL` pointe bien vers Railway
- Vérifiez les logs de build sur Vercel
- Testez l'URL de l'API : `https://votre-api.railway.app/health`

### Erreurs CORS
- Vérifiez que l'URL Vercel est dans `AllowedOrigins__0`
- Redéployez le backend Railway après avoir ajouté la variable

## Coûts

- **Vercel :** Gratuit (100 GB bandwidth/mois)
- **Railway :** $5/mois de crédit gratuit, largement suffisant pour commencer

## Prochaines étapes

Dites-moi quand vous êtes prêt et je vous guiderai pour chaque étape.
