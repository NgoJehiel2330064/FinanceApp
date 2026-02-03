# ?? Guide Complet : Résoudre "Failed to Fetch" Définitivement

## ?? Table des Matières
1. [Comprendre le Problème](#comprendre-le-problème)
2. [Solution Rapide](#solution-rapide)
3. [Scripts Automatiques](#scripts-automatiques)
4. [Résolution Manuelle](#résolution-manuelle)
5. [Prévention Future](#prévention-future)

---

## ?? Comprendre le Problème

### Qu'est-ce que "Failed to Fetch" ?

Cette erreur signifie que le **frontend** (Next.js) ne peut pas se connecter au **backend** (.NET API).

```
Frontend (localhost:3000) ? Backend (localhost:5153)
```

### Causes Principales

| Cause | Symptôme | Solution |
|-------|----------|----------|
| **Backend non démarré** | Port 5153 fermé | Démarrer le backend |
| **Port incorrect** | Frontend appelle le mauvais port | Corriger `.env.local` |
| **CORS non configuré** | Erreur CORS dans console | Corriger `Program.cs` |
| **PostgreSQL arrêté** | Erreur de connexion DB | Démarrer Docker |

---

## ? Solution Rapide (2 minutes)

### Option 1 : Script Automatique (Recommandé)

```powershell
# 1. Vérifier la synchronisation
.\check-sync.ps1

# 2. Si problème détecté, corriger automatiquement
.\fix-sync.ps1

# 3. Démarrer tous les services
.\start-both.ps1
```

**Résultat** : Backend + Frontend démarrés automatiquement avec les bons ports.

### Option 2 : Démarrage Manuel Rapide

```powershell
# Terminal 1 : Backend
cd FinanceApp
dotnet run --launch-profile http

# Terminal 2 : Frontend
cd finance-ui
npm run dev
```

---

## ?? Scripts Automatiques

### ?? `start-both.ps1` - Démarrage Complet

**Ce qu'il fait :**
- ? Démarre PostgreSQL (Docker)
- ? Applique les migrations EF Core
- ? Démarre le backend sur port 5153
- ? Vérifie la configuration frontend
- ? Démarre le frontend sur port 3000
- ? Ouvre automatiquement le navigateur

**Usage :**
```powershell
.\start-both.ps1
```

**Résultat :**
- 3 fenêtres PowerShell s'ouvrent :
  1. Backend .NET
  2. Frontend Next.js
  3. Console principale

---

### ?? `stop-both.ps1` - Arrêt Complet

**Ce qu'il fait :**
- ? Arrête le frontend (port 3000)
- ? Arrête le backend (port 5153)
- ? Arrête PostgreSQL (Docker)
- ? Ferme les fenêtres PowerShell

**Usage :**
```powershell
.\stop-both.ps1
```

---

### ?? `check-sync.ps1` - Vérification de Configuration

**Ce qu'il vérifie :**
- ? Port backend dans `launchSettings.json`
- ? URL API dans `.env.local`
- ? Configuration CORS dans `Program.cs`
- ? Services en cours d'exécution
- ? Connectivité API

**Usage :**
```powershell
.\check-sync.ps1
```

**Exemple de résultat :**
```
? Backend : Port 5153 ?
? Frontend : URL API correcte ?
? CORS : localhost:3000 autorisé ?
? Backend : En cours d'exécution sur le port 5153 ?
? Frontend : En cours d'exécution sur le port 3000 ?
? PostgreSQL : En cours d'exécution ?
? API : Répond correctement ?

? CONFIGURATION PARFAITEMENT SYNCHRONISÉE
```

---

### ?? `fix-sync.ps1` - Correction Automatique

**Ce qu'il fait :**
- ? Corrige `.env.local` pour pointer vers `http://localhost:5153`
- ? Vérifie `launchSettings.json`
- ? Vérifie `api-config.ts`
- ? Vérifie la configuration CORS

**Usage :**
```powershell
.\fix-sync.ps1
```

**Exemple de résultat :**
```
? Frontend : Configuration corrigée (http://localhost:5153)
? Backend : Configuration correcte (http://localhost:5153)
? API Config : Utilise correctement les variables d'environnement ?
? CORS : localhost:3000 autorisé ?

? CORRECTIONS APPLIQUÉES
```

---

## ??? Résolution Manuelle

### Étape 1 : Vérifier les Ports

#### Backend (launchSettings.json)
```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5153"  // ?? Vérifier ce port
    }
  }
}
```

#### Frontend (.env.local)
```env
NEXT_PUBLIC_API_URL=http://localhost:5153  # ?? Doit correspondre au backend
```

**Vérification :**
```powershell
# Voir le contenu de .env.local
Get-Content finance-ui\.env.local
```

---

### Étape 2 : Vérifier CORS (Program.cs)

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000"  // ?? Port du frontend Next.js
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Plus bas dans le fichier
app.UseCors("AllowFrontend");  // ?? Ne pas oublier !
```

---

### Étape 3 : Vérifier que les Ports Sont Libres

```powershell
# Vérifier le port 5153 (Backend)
Test-NetConnection -ComputerName localhost -Port 5153

# Vérifier le port 3000 (Frontend)
Test-NetConnection -ComputerName localhost -Port 3000
```

**Si un port est occupé :**
```powershell
# Voir quel processus utilise le port 5153
Get-NetTCPConnection -LocalPort 5153 | 
    Select-Object -Property State, OwningProcess | 
    ForEach-Object { Get-Process -Id $_.OwningProcess }

# Arrêter le processus (remplacer PID par l'ID du processus)
Stop-Process -Id <PID> -Force
```

---

### Étape 4 : Démarrer PostgreSQL

```powershell
# Vérifier si PostgreSQL est en cours d'exécution
docker ps

# Si non présent, démarrer
docker-compose up -d

# Attendre que PostgreSQL soit prêt
Start-Sleep -Seconds 5
```

---

### Étape 5 : Démarrer le Backend

```powershell
cd FinanceApp

# Appliquer les migrations (première fois)
dotnet ef database update

# Démarrer le backend
dotnet run --launch-profile http
```

**Vérification :**
- Une fenêtre de navigateur s'ouvre sur Swagger : `http://localhost:5153/swagger`
- Testez un endpoint : `GET /api/transactions`

---

### Étape 6 : Démarrer le Frontend

**Dans un NOUVEAU terminal :**
```powershell
cd finance-ui

# Vérifier que node_modules est installé
npm install

# Démarrer le frontend
npm run dev
```

**Vérification :**
- Le terminal affiche : `Local: http://localhost:3000`
- Ouvrez `http://localhost:3000` dans votre navigateur

---

## ?? Workflow Quotidien

### Démarrage du Matin

```powershell
# Option 1 : Script automatique (recommandé)
.\start-both.ps1

# Option 2 : Manuel
# Terminal 1
cd FinanceApp && dotnet run --launch-profile http

# Terminal 2
cd finance-ui && npm run dev
```

### Arrêt le Soir

```powershell
# Option 1 : Script automatique
.\stop-both.ps1

# Option 2 : Manuel
# Appuyez sur Ctrl+C dans chaque terminal
# Arrêter PostgreSQL :
docker stop postgres
```

---

## ?? Dépannage Avancé

### Problème : "Port 5153 already in use"

```powershell
# Trouver le processus
Get-NetTCPConnection -LocalPort 5153 | 
    Select-Object -ExpandProperty OwningProcess | 
    ForEach-Object { Get-Process -Id $_ }

# Arrêter le processus
Stop-Process -Id <PID> -Force
```

### Problème : "CORS Error"

**Symptôme :** Dans la console du navigateur :
```
Access to fetch at 'http://localhost:5153/api/transactions' from origin 
'http://localhost:3000' has been blocked by CORS policy
```

**Solution :**
1. Ouvrez `FinanceApp/Program.cs`
2. Vérifiez que `app.UseCors("AllowFrontend")` est **avant** `app.MapControllers()`
3. Vérifiez que `"http://localhost:3000"` est dans `WithOrigins()`

### Problème : PostgreSQL ne démarre pas

```powershell
# Vérifier les logs
docker logs postgres

# Recréer le conteneur
docker-compose down
docker-compose up -d
```

### Problème : Migrations EF Core

```powershell
cd FinanceApp

# Vérifier les migrations disponibles
dotnet ef migrations list

# Appliquer toutes les migrations
dotnet ef database update

# Si erreur, recréer la base de données
dotnet ef database drop
dotnet ef database update
```

---

## ?? Checklist de Vérification

Avant de signaler un bug, vérifiez :

- [ ] PostgreSQL est démarré (`docker ps`)
- [ ] Backend est démarré (`Test-NetConnection -Port 5153`)
- [ ] Frontend est démarré (`Test-NetConnection -Port 3000`)
- [ ] `.env.local` contient `NEXT_PUBLIC_API_URL=http://localhost:5153`
- [ ] `launchSettings.json` utilise le port 5153
- [ ] CORS est configuré pour `localhost:3000`
- [ ] Migrations EF Core sont appliquées
- [ ] Aucune erreur dans les logs backend
- [ ] Aucune erreur dans les logs frontend

---

## ?? Comprendre la Configuration

### Flux de Données

```
???????????????????????????????????????????????????????????
?                    USER (Browser)                        ?
?                 http://localhost:3000                    ?
???????????????????????????????????????????????????????????
                       ?
                       ? fetch(API_URL + '/api/transactions')
                       ?
???????????????????????????????????????????????????????????
?              FRONTEND (Next.js)                          ?
?              Port: 3000                                  ?
?              Config: .env.local                          ?
?              NEXT_PUBLIC_API_URL=http://localhost:5153   ?
???????????????????????????????????????????????????????????
                       ?
                       ? HTTP Request
                       ?
???????????????????????????????????????????????????????????
?              BACKEND (.NET API)                          ?
?              Port: 5153                                  ?
?              Config: launchSettings.json                 ?
?              CORS: AllowFrontend (localhost:3000)        ?
???????????????????????????????????????????????????????????
                       ?
                       ? SQL Queries
                       ?
???????????????????????????????????????????????????????????
?              PostgreSQL (Docker)                         ?
?              Port: 5432                                  ?
?              Database: finance_db                        ?
???????????????????????????????????????????????????????????
```

### Variables d'Environnement

| Variable | Fichier | Usage |
|----------|---------|-------|
| `NEXT_PUBLIC_API_URL` | `.env.local` | URL du backend pour le frontend |
| `ASPNETCORE_ENVIRONMENT` | `launchSettings.json` | Mode de l'API (Development/Production) |
| `Gemini:ApiKey` | User Secrets | Clé API Gemini pour l'IA |

---

## ?? Bonnes Pratiques

### ? À Faire

1. **Toujours vérifier** la synchronisation avant de coder
   ```powershell
   .\check-sync.ps1
   ```

2. **Utiliser les scripts** automatiques
   ```powershell
   .\start-both.ps1  # Démarrage
   .\stop-both.ps1   # Arrêt
   ```

3. **Commiter `.env.example`** (pas `.env.local`)
   ```env
   # .env.example
   NEXT_PUBLIC_API_URL=http://localhost:5153
   NEXT_PUBLIC_GEMINI_API_KEY=your_key_here
   ```

4. **Tester avec Swagger** avant le frontend
   - Ouvrir `http://localhost:5153/swagger`
   - Tester `GET /api/transactions`

### ? À Éviter

1. ? Changer les ports sans vérifier la synchronisation
2. ? Commiter `.env.local` (contient des secrets)
3. ? Oublier de démarrer PostgreSQL
4. ? Utiliser des ports différents entre `launchSettings.json` et `.env.local`

---

## ?? Ressources

### Fichiers de Configuration

| Fichier | Rôle | Port Configuré |
|---------|------|----------------|
| `FinanceApp/Properties/launchSettings.json` | Port backend | 5153 |
| `finance-ui/.env.local` | URL API frontend | http://localhost:5153 |
| `FinanceApp/Program.cs` | CORS backend | localhost:3000 |
| `finance-ui/lib/api-config.ts` | Configuration API | process.env.NEXT_PUBLIC_API_URL |

### Scripts Disponibles

| Script | Description |
|--------|-------------|
| `start-both.ps1` | Démarre tout (PostgreSQL + Backend + Frontend) |
| `stop-both.ps1` | Arrête tout |
| `check-sync.ps1` | Vérifie la synchronisation |
| `fix-sync.ps1` | Corrige automatiquement la configuration |

---

## ?? Résumé

Pour **ne plus jamais avoir "Failed to Fetch"** :

1. ? Utilisez `.\start-both.ps1` pour démarrer
2. ? Utilisez `.\check-sync.ps1` avant de coder
3. ? Utilisez `.\fix-sync.ps1` si problème
4. ? Utilisez `.\stop-both.ps1` pour arrêter

**C'est tout !** ??

---

**Créé le :** 3 février 2025  
**Dernière mise à jour :** 3 février 2025  
**Version :** 1.0
