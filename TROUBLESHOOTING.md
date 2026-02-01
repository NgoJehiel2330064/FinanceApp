# ?? Guide de Dépannage - FinanceApp

Ce document vous aide à résoudre les problèmes courants rencontrés avec l'application.

---

## ?? Erreur : "Failed to fetch" dans le Frontend

### Symptômes
```
TypeError: Failed to fetch
  at fetchTransactions (app/page.tsx:88:32)
```

### Causes possibles

#### 1?? L'API ASP.NET Core n'est pas lancée

**Vérification** :
```bash
# Vérifier si le port 5152 est occupé
netstat -ano | findstr :5152
```

**Solution** :
```bash
# Lancer l'API
cd FinanceApp
dotnet run
```

L'API devrait afficher :
```
Now listening on: http://localhost:5152
Now listening on: https://localhost:7219
```

---

#### 2?? Le port 5152 est occupé par une ancienne instance

**Symptômes** :
```
System.IO.IOException: Failed to bind to address http://127.0.0.1:5152: address already in use
```

**Solution** :
```bash
# 1. Trouver le processus qui occupe le port
netstat -ano | findstr :5152

# 2. Noter le PID (exemple: 33024)
# 3. Terminer le processus
taskkill /F /PID 33024

# 4. Relancer l'API
cd FinanceApp
dotnet run
```

---

#### 3?? Problème CORS (Cross-Origin Resource Sharing)

**Symptômes dans la console du navigateur** :
```
Access to fetch at 'http://localhost:5152/api/transactions' from origin 
'http://localhost:3000' has been blocked by CORS policy
```

**Vérification** :
- Vérifiez sur quel port tourne votre frontend Next.js (généralement 3000 ou 3001)
- Ouvrez la console développeur du navigateur (F12)
- Regardez l'onglet "Network" pour voir les détails de l'erreur

**Solution 1 : Ajouter le port dans Program.cs**

Éditez `FinanceApp/Program.cs` :

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:3000",  // Ajoutez votre port ici
                  "http://localhost:3001",
                  "http://localhost:VOTRE_PORT"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

**Solution 2 : Mode développement - Autoriser tous les domaines** (?? Temporaire seulement)

Dans `Program.cs`, changez la ligne :
```csharp
// Avant :
app.UseCors("AllowFrontend");

// Après (UNIQUEMENT POUR TESTER) :
app.UseCors("AllowAll");
```

**?? IMPORTANT** : Ne jamais utiliser `AllowAll` en production !

---

#### 4?? URL incorrecte dans le Frontend

**Vérification dans votre code Next.js** :

Votre fichier `app/page.tsx` ou configuration doit pointer vers :
```typescript
const API_BASE_URL = "http://localhost:5152";

// Endpoint transactions
const transactionsUrl = `${API_BASE_URL}/api/transactions`;
```

**URLs correctes** :
- ? `http://localhost:5152/api/transactions`
- ? `http://localhost:5152/api/finance/advice`
- ? `http://localhost:5000/api/transactions` (mauvais port)
- ? `http://localhost:5152/transactions` (manque `/api`)

---

#### 5?? PostgreSQL n'est pas démarré (pour /api/transactions)

**Vérification** :
```bash
docker ps
```

Vous devriez voir un conteneur `postgres_db` en cours d'exécution.

**Solution** :
```bash
# Démarrer Docker Compose
docker-compose up -d

# Vérifier que PostgreSQL est démarré
docker ps

# Vérifier les logs
docker logs postgres_db
```

---

## ??? Erreur : "Cannot connect to PostgreSQL"

### Symptômes
```
Npgsql.NpgsqlException: Connection refused
```

### Solutions

#### 1?? Démarrer PostgreSQL
```bash
docker-compose up -d
```

#### 2?? Vérifier la configuration
```bash
# Tester la connexion manuellement
docker exec -it postgres_db psql -U postgres -d finance_db

# Si ça fonctionne, vous êtes connecté à la base
```

#### 3?? Réinitialiser la base de données
```bash
# Arrêter et supprimer les volumes
docker-compose down -v

# Redémarrer
docker-compose up -d

# Appliquer les migrations
cd FinanceApp
dotnet ef database update
```

---

## ?? Erreur : "Clé API Gemini non configurée"

### Symptômes
```json
{
  "advice": "Impossible de générer un conseil pour le moment. Vérifiez votre configuration."
}
```

### Solution
```bash
cd FinanceApp

# Configurer la clé API avec User Secrets
dotnet user-secrets set "Gemini:ApiKey" "VOTRE_CLE_API_GEMINI"

# Vérifier
dotnet user-secrets list
```

**Obtenir une clé gratuite** : [Google AI Studio](https://makersuite.google.com/app/apikey)

---

## ?? Tester l'API manuellement

### Avec PowerShell

```powershell
# GET - Liste des transactions
Invoke-RestMethod -Uri "http://localhost:5152/api/transactions" -Method Get

# POST - Créer une transaction
$body = @{
    description = "Test Transaction"
    amount = 100
    type = "Expense"
    category = "Test"
    date = (Get-Date).ToString("o")
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5152/api/transactions" `
                  -Method Post `
                  -Body $body `
                  -ContentType "application/json"

# GET - Conseil financier IA
Invoke-RestMethod -Uri "http://localhost:5152/api/finance/advice" -Method Get
```

### Avec curl (Git Bash / Linux / macOS)

```bash
# GET - Liste des transactions
curl http://localhost:5152/api/transactions

# POST - Créer une transaction
curl -X POST http://localhost:5152/api/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Test Transaction",
    "amount": 100,
    "type": "Expense",
    "category": "Test",
    "date": "2025-02-01T00:00:00Z"
  }'

# GET - Conseil financier IA
curl http://localhost:5152/api/finance/advice
```

### Avec un navigateur

Ouvrez simplement :
- http://localhost:5152/swagger
- http://localhost:5152/api/transactions
- http://localhost:5152/api/finance/advice

---

## ?? Checklist de dépannage

Suivez cette checklist dans l'ordre :

- [ ] **1. PostgreSQL est démarré** : `docker ps` montre `postgres_db`
- [ ] **2. API est lancée** : `dotnet run` dans le dossier `FinanceApp`
- [ ] **3. API répond** : Ouvrir http://localhost:5152/swagger
- [ ] **4. Transactions fonctionnent** : Tester GET http://localhost:5152/api/transactions
- [ ] **5. Frontend sur le bon port** : Vérifier que Next.js tourne sur 3000 ou 3001
- [ ] **6. URL correcte dans le frontend** : `http://localhost:5152/api/...`
- [ ] **7. CORS configuré** : Le port du frontend est dans `Program.cs`
- [ ] **8. Console navigateur** : Aucune erreur CORS visible (F12)

---

## ?? Logs utiles

### Logs de l'API ASP.NET Core

Les logs s'affichent dans le terminal où vous avez lancé `dotnet run`.

**Activer les logs détaillés** (temporaire) :

Éditez `appsettings.Development.json` :
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Logs PostgreSQL

```bash
# Voir les logs en temps réel
docker logs -f postgres_db

# Voir les dernières 100 lignes
docker logs --tail 100 postgres_db
```

### Logs Docker

```bash
# Voir tous les conteneurs (même arrêtés)
docker ps -a

# Voir les logs d'un conteneur
docker logs postgres_db
```

---

## ?? Réinitialisation complète

Si rien ne fonctionne, réinitialisez tout :

```bash
# 1. Arrêter tout
docker-compose down -v
taskkill /F /IM dotnet.exe

# 2. Nettoyer le build .NET
cd FinanceApp
dotnet clean
Remove-Item -Recurse -Force bin, obj

# 3. Redémarrer PostgreSQL
cd ..
docker-compose up -d

# 4. Restaurer les packages
cd FinanceApp
dotnet restore

# 5. Appliquer les migrations
dotnet ef database update

# 6. Relancer l'API
dotnet run
```

---

## ?? Besoin d'aide ?

Si le problème persiste :

1. **Vérifiez les logs** (API + Docker + Console navigateur)
2. **Testez l'API avec Swagger** : http://localhost:5152/swagger
3. **Créez une issue** sur GitHub avec :
   - Le message d'erreur complet
   - Les logs de l'API
   - Les logs du navigateur (F12 ? Console)
   - Votre configuration (ports, OS, etc.)

---

## ?? Ports utilisés par l'application

| Service | Port | URL |
|---------|------|-----|
| **API HTTP** | 5152 | http://localhost:5152 |
| **API HTTPS** | 7219 | https://localhost:7219 |
| **PostgreSQL** | 5432 | localhost:5432 |
| **Next.js (frontend)** | 3000 | http://localhost:3000 |
| **Swagger UI** | 5152 | http://localhost:5152/swagger |

---

**?? Astuce** : Gardez ce fichier ouvert dans un onglet pendant le développement !
