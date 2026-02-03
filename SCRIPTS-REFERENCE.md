# ?? Scripts de Gestion FinanceApp

## ?? Scripts Disponibles

### ?? Démarrage

```powershell
# Démarrer TOUT (PostgreSQL + Backend + Frontend)
.\start-both.ps1
```

**Résultat :**
- ? 3 fenêtres PowerShell s'ouvrent
- ? PostgreSQL démarre (Docker)
- ? Backend sur http://localhost:5153
- ? Frontend sur http://localhost:3000
- ? Navigateur s'ouvre automatiquement

---

### ?? Arrêt

```powershell
# Arrêter TOUT
.\stop-both.ps1
```

**Résultat :**
- ? Frontend arrêté (port 3000)
- ? Backend arrêté (port 5153)
- ? PostgreSQL arrêté
- ? Fenêtres PowerShell fermées

---

### ?? Vérification

```powershell
# Vérifier la synchronisation Backend/Frontend
.\check-sync.ps1
```

**Ce qui est vérifié :**
- ? Port backend (5153)
- ? URL API frontend
- ? Configuration CORS
- ? Services en cours
- ? Connectivité API

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

### ?? Correction

```powershell
# Corriger automatiquement la configuration
.\fix-sync.ps1
```

**Ce qui est corrigé :**
- ? `.env.local` ? Port 5153
- ? Vérification `launchSettings.json`
- ? Vérification CORS
- ? Vérification `api-config.ts`

---

## ?? Workflow Quotidien

### Matin (Démarrage)

```powershell
# 1. Vérifier la synchronisation
.\check-sync.ps1

# 2. Tout démarrer
.\start-both.ps1

# 3. Coder ! ??
```

### Soir (Arrêt)

```powershell
# Tout arrêter
.\stop-both.ps1
```

---

## ?? En Cas d'Erreur "Failed to Fetch"

### Solution Rapide

```powershell
# 1. Arrêter tout
.\stop-both.ps1

# 2. Vérifier et corriger
.\check-sync.ps1
.\fix-sync.ps1

# 3. Redémarrer
.\start-both.ps1
```

### Diagnostic

```powershell
# Vérifier les ports
Test-NetConnection -Port 5153  # Backend
Test-NetConnection -Port 3000  # Frontend
Test-NetConnection -Port 5432  # PostgreSQL

# Voir les processus
Get-NetTCPConnection -LocalPort 5153
Get-NetTCPConnection -LocalPort 3000
```

---

## ?? Architecture des Ports

```
???????????????????????????????????????????
?  Frontend Next.js (Port 3000)           ?
?  Fichier : finance-ui/.env.local        ?
?  Config : NEXT_PUBLIC_API_URL=          ?
?           http://localhost:5153         ?
???????????????????????????????????????????
               ?
               ? HTTP Requests
               ?
???????????????????????????????????????????
?  Backend .NET API (Port 5153)           ?
?  Fichier : Properties/launchSettings    ?
?  Config : applicationUrl=               ?
?           http://localhost:5153         ?
?  CORS : AllowOrigin localhost:3000      ?
???????????????????????????????????????????
               ?
               ? SQL Queries
               ?
???????????????????????????????????????????
?  PostgreSQL Docker (Port 5432)          ?
?  Base : finance_db                      ?
???????????????????????????????????????????
```

---

## ?? Comprendre les Scripts

### start-both.ps1

**Étapes :**
1. Vérifier Docker
2. Démarrer PostgreSQL
3. Vérifier les ports
4. Appliquer les migrations EF Core
5. Démarrer le backend (nouvelle fenêtre)
6. Vérifier `.env.local`
7. Démarrer le frontend (nouvelle fenêtre)
8. Ouvrir le navigateur

**Durée :** ~10 secondes

---

### stop-both.ps1

**Étapes :**
1. Arrêter le frontend (port 3000)
2. Arrêter le backend (port 5153)
3. Arrêter PostgreSQL (Docker)
4. Fermer les fenêtres PowerShell

**Durée :** ~3 secondes

---

### check-sync.ps1

**Vérifications :**
1. `launchSettings.json` ? Port backend
2. `.env.local` ? URL API
3. `Program.cs` ? CORS
4. Ports ouverts (5153, 3000, 5432)
5. Connectivité API

**Durée :** ~2 secondes

---

### fix-sync.ps1

**Actions :**
1. Corriger `.env.local`
2. Vérifier `launchSettings.json`
3. Vérifier `api-config.ts`
4. Vérifier CORS

**Durée :** ~1 seconde

---

## ?? Conseils

### ? Bonnes Pratiques

1. **Toujours vérifier** avant de coder
   ```powershell
   .\check-sync.ps1
   ```

2. **Utiliser start-both.ps1** au lieu de démarrer manuellement
   ```powershell
   .\start-both.ps1
   ```

3. **Arrêter proprement** le soir
   ```powershell
   .\stop-both.ps1
   ```

### ? À Éviter

1. ? Changer les ports sans vérifier
2. ? Démarrer le frontend sans le backend
3. ? Oublier de démarrer PostgreSQL
4. ? Modifier `.env.local` manuellement (utiliser `fix-sync.ps1`)

---

## ?? Documentation

Pour plus de détails, consultez :
- **[GUIDE-SYNCHRONISATION.md](GUIDE-SYNCHRONISATION.md)** - Guide complet
- **[RAPPORT-SYNCHRONISATION-COMPLET.md](RAPPORT-SYNCHRONISATION-COMPLET.md)** - Analyse technique

---

## ?? Aide

### Erreur "Port already in use"

```powershell
# Voir quel processus utilise le port
Get-NetTCPConnection -LocalPort 5153 | 
    Select-Object -ExpandProperty OwningProcess |
    ForEach-Object { Get-Process -Id $_ }

# Arrêter le processus
Stop-Process -Id <PID> -Force
```

### Erreur "Docker not found"

```powershell
# Démarrer Docker Desktop manuellement
# Puis relancer
.\start-both.ps1
```

### Erreur "Failed to fetch"

```powershell
# Solution complète
.\stop-both.ps1
.\fix-sync.ps1
.\start-both.ps1
```

---

**Créé le :** 3 février 2025  
**Version :** 1.0
