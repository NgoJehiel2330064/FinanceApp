# ?? Guide de Démarrage Rapide - FinanceApp

## ? Votre configuration actuelle

| Composant | État |
|-----------|------|
| **Clé API Gemini** | ? Configurée (AIzaSyDfU2oI...) |
| **PostgreSQL** | ? En cours d'exécution (conteneur `postgres_db`) |
| **Port 5152** | ? Libre |
| **User Secrets** | ? Configurés |

---

## ?? Démarrer l'application (3 options)

### Option 1 : Script automatique (Recommandé) ??

```powershell
.\start-app.ps1
```

Ce script va :
- ? Vérifier tous les prérequis
- ? Démarrer PostgreSQL (déjà fait !)
- ? Libérer le port 5152 si nécessaire
- ? Appliquer les migrations
- ? Lancer l'API

---

### Option 2 : Commandes manuelles

```powershell
# 1. S'assurer que PostgreSQL tourne (déjà fait !)
docker ps  # Vous devriez voir postgres_db

# 2. Aller dans le dossier du projet
cd FinanceApp

# 3. Appliquer les migrations (première fois seulement)
dotnet ef database update

# 4. Lancer l'API
dotnet run
```

---

### Option 3 : Visual Studio / Rider

1. Ouvrez `FinanceApp.sln`
2. Appuyez sur **F5** (ou cliquez sur ?? Run)
3. L'API démarrera automatiquement

---

## ?? URLs disponibles

Une fois l'API lancée, vous aurez accès à :

| Service | URL |
|---------|-----|
| **API HTTP** | http://localhost:5152 |
| **API HTTPS** | https://localhost:7219 |
| **Swagger UI** | http://localhost:5152/swagger |
| **PostgreSQL** | localhost:5432 |

---

## ?? Tester l'API

### 1. Avec Swagger (Interface graphique)

Ouvrez votre navigateur : **http://localhost:5152/swagger**

Vous verrez une interface interactive pour tester tous les endpoints.

---

### 2. Avec PowerShell

```powershell
# GET - Liste des transactions
Invoke-RestMethod -Uri "http://localhost:5152/api/transactions" -Method Get

# POST - Créer une transaction
$transaction = @{
    description = "Salaire mensuel"
    amount = 3000
    type = "Income"
    category = "Salaire"
    date = (Get-Date).ToString("o")
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5152/api/transactions" `
                  -Method Post `
                  -Body $transaction `
                  -ContentType "application/json"

# GET - Conseil financier IA (Gemini)
Invoke-RestMethod -Uri "http://localhost:5152/api/finance/advice" -Method Get
```

---

### 3. Avec curl (Git Bash / Linux)

```bash
# GET - Liste des transactions
curl http://localhost:5152/api/transactions

# POST - Créer une transaction
curl -X POST http://localhost:5152/api/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Salaire mensuel",
    "amount": 3000,
    "type": "Income",
    "category": "Salaire",
    "date": "2025-02-01T00:00:00Z"
  }'

# GET - Conseil financier IA
curl http://localhost:5152/api/finance/advice
```

---

### 4. Avec votre navigateur

Ouvrez simplement ces URLs :
- http://localhost:5152/api/transactions
- http://localhost:5152/api/finance/advice

---

## ?? Endpoints disponibles

### Transactions

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| `GET` | `/api/transactions` | Liste toutes les transactions |
| `GET` | `/api/transactions/{id}` | Détails d'une transaction |
| `POST` | `/api/transactions` | Créer une transaction |
| `PUT` | `/api/transactions/{id}` | Modifier une transaction |
| `DELETE` | `/api/transactions/{id}` | Supprimer une transaction |
| `GET` | `/api/transactions/balance` | Calculer le solde total |

### Finance (IA Gemini)

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| `GET` | `/api/finance/advice` | Conseil financier personnalisé |

---

## ?? Connecter votre Frontend

Si vous avez un frontend Next.js, React, Vue, etc., configurez l'URL de l'API :

```typescript
// config/api.ts
const API_BASE_URL = "http://localhost:5152";

export const API_CONFIG = {
  BASE_URL: API_BASE_URL,
  ENDPOINTS: {
    TRANSACTIONS: "/api/transactions",
    FINANCIAL_ADVICE: "/api/finance/advice",
  }
};
```

**Consultez `FRONTEND-CONFIGURATION.md`** pour des exemples complets.

---

## ?? Arrêter l'application

### Avec le script

```powershell
.\stop-app.ps1
```

### Manuellement

```powershell
# 1. Dans le terminal de l'API : Ctrl+C

# 2. Arrêter PostgreSQL (optionnel)
docker-compose down
```

---

## ? Problèmes ?

### Erreur : "Port 5152 already in use"

```powershell
# Trouver le processus
netstat -ano | findstr :5152

# Tuer le processus (remplacez PID par le numéro trouvé)
taskkill /F /PID <PID>
```

### Erreur : "Cannot connect to PostgreSQL"

```powershell
# Vérifier que PostgreSQL tourne
docker ps

# Si pas démarré
docker-compose up -d

# Vérifier les logs
docker logs postgres_db
```

### Erreur : "Clé API Gemini non configurée"

```powershell
cd FinanceApp
dotnet user-secrets set "Gemini:ApiKey" "AIzaSyDfU2oIqH7WQ825btkeddIWONEPnApF8Gs"
```

### Erreur frontend : "Failed to fetch"

1. ? Vérifiez que l'API est démarrée : http://localhost:5152/swagger
2. ? Vérifiez l'URL dans votre frontend : doit être `http://localhost:5152`
3. ? Consultez **TROUBLESHOOTING.md** pour les problèmes CORS

---

## ?? Documentation complète

- **[README.md](README.md)** - Vue d'ensemble du projet
- **[SECRETS-CONFIGURATION.md](SECRETS-CONFIGURATION.md)** - Gestion des clés API
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** - Résolution de problèmes détaillée
- **[FRONTEND-CONFIGURATION.md](FRONTEND-CONFIGURATION.md)** - Configuration du frontend
- **[test-config.ps1](test-config.ps1)** - Script de vérification de la configuration

---

## ? Prêt à coder !

Votre environnement est **100% configuré** ! ??

```powershell
# Lancez l'application
.\start-app.ps1

# Ouvrez Swagger
start http://localhost:5152/swagger

# Commencez à développer votre frontend
# (l'API est prête à recevoir vos requêtes !)
```

**Bon développement ! ??**
