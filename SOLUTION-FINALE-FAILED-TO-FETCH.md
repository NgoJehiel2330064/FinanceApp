# ?? RÉSOLUTION DÉFINITIVE : "Failed to Fetch"

## ?? Résumé Exécutif

Vous avez maintenant **4 scripts PowerShell** qui résolvent automatiquement le problème de synchronisation backend/frontend.

---

## ? Ce Qui A Été Créé

### 1. **start-both.ps1** - Démarrage Automatique

**Usage :**
```powershell
.\start-both.ps1
```

**Ce qu'il fait :**
- ? Démarre PostgreSQL (Docker)
- ? Applique les migrations EF Core
- ? Démarre le backend sur port **5153**
- ? Vérifie et corrige `.env.local` si nécessaire
- ? Démarre le frontend sur port **3000**
- ? Ouvre automatiquement http://localhost:3000

**Résultat :** 3 fenêtres PowerShell s'ouvrent (Backend, Frontend, Console)

---

### 2. **stop-both.ps1** - Arrêt Automatique

**Usage :**
```powershell
.\stop-both.ps1
```

**Ce qu'il fait :**
- ? Arrête le frontend (port 3000)
- ? Arrête le backend (port 5153)
- ? Arrête PostgreSQL (Docker)
- ? Ferme les fenêtres PowerShell

---

### 3. **check-sync.ps1** - Vérification de Configuration

**Usage :**
```powershell
.\check-sync.ps1
```

**Ce qu'il vérifie :**
- ? Port backend dans `launchSettings.json`
- ? URL API dans `.env.local`
- ? Configuration CORS
- ? Services en cours d'exécution
- ? Connectivité API

**Exemple de résultat :**
```
? Backend : Port 5153 ?
? Frontend : URL API correcte ?
? CORS : localhost:3000 autorisé ?
? Backend : En cours d'exécution sur le port 5153 ?
? Frontend : En cours d'exécution sur le port 3000 ?
? PostgreSQL : En cours d'exécution ?

? CONFIGURATION PARFAITEMENT SYNCHRONISÉE
```

---

### 4. **fix-sync.ps1** - Correction Automatique

**Usage :**
```powershell
.\fix-sync.ps1
```

**Ce qu'il fait :**
- ? Corrige `.env.local` ? `NEXT_PUBLIC_API_URL=http://localhost:5153`
- ? Vérifie `launchSettings.json`
- ? Vérifie `api-config.ts`
- ? Vérifie la configuration CORS

---

## ?? Comment Résoudre "Failed to Fetch" Maintenant

### Solution en 3 Commandes

```powershell
# 1. Arrêter tout
.\stop-both.ps1

# 2. Vérifier et corriger la configuration
.\check-sync.ps1
.\fix-sync.ps1

# 3. Redémarrer tout
.\start-both.ps1
```

**Durée totale :** ~15 secondes

---

## ?? Votre Nouveau Workflow

### Démarrage du Projet (Tous les jours)

```powershell
# UNE SEULE COMMANDE !
.\start-both.ps1
```

**Résultat :**
- Backend prêt : http://localhost:5153
- Frontend prêt : http://localhost:3000
- PostgreSQL prêt
- Navigateur ouvert automatiquement

### Arrêt du Projet (Le soir)

```powershell
# UNE SEULE COMMANDE !
.\stop-both.ps1
```

### En Cas d'Erreur

```powershell
# Vérifier la configuration
.\check-sync.ps1

# Si problème détecté
.\fix-sync.ps1

# Redémarrer
.\start-both.ps1
```

---

## ?? Comparaison Avant/Après

### ? Avant (Workflow Manuel)

```powershell
# Terminal 1 : Démarrer PostgreSQL
docker-compose up -d

# Terminal 2 : Démarrer Backend
cd FinanceApp
dotnet ef database update
dotnet run --launch-profile http

# Terminal 3 : Démarrer Frontend
cd finance-ui
npm run dev

# Ouvrir le navigateur manuellement
# http://localhost:3000
```

**Problèmes :**
- ? 3 terminaux à gérer
- ? Risque d'oublier PostgreSQL
- ? Port incohérent si mal configuré
- ? Erreur "Failed to Fetch" fréquente

---

### ? Après (Workflow Automatisé)

```powershell
# UNE SEULE COMMANDE
.\start-both.ps1
```

**Avantages :**
- ? Tout démarre automatiquement
- ? Configuration vérifiée
- ? Ports synchronisés
- ? Navigateur ouvert automatiquement
- ? Plus d'erreur "Failed to Fetch"

---

## ?? Comprendre la Solution

### Pourquoi "Failed to Fetch" ?

```
Frontend (localhost:3000) ????? Backend (pas démarré ou port incorrect)
```

Le frontend Next.js essaie de se connecter au backend .NET, mais :
1. Le backend n'est pas démarré
2. Le backend écoute sur un port différent
3. CORS non configuré

### Comment les Scripts Résolvent le Problème

```
Frontend (localhost:3000) ????? Backend (localhost:5153)
                                      ?
                                      ? CORS configuré
                                      ?
                                      ? PostgreSQL démarré
```

**start-both.ps1** garantit que :
1. PostgreSQL est démarré **en premier**
2. Backend démarre sur le **bon port (5153)**
3. Frontend est configuré pour appeler le **bon port (5153)**
4. CORS autorise les requêtes depuis **localhost:3000**

---

## ?? Fichiers de Configuration

### Backend (launchSettings.json)

```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5153"  // ?? Port backend
    }
  }
}
```

### Frontend (.env.local)

```env
NEXT_PUBLIC_API_URL=http://localhost:5153  # ?? Doit correspondre au backend
```

### Backend CORS (Program.cs)

```csharp
policy.WithOrigins(
    "http://localhost:3000"  // ?? Port frontend
)
```

**Les scripts vérifient automatiquement que ces 3 fichiers sont synchronisés.**

---

## ?? Conseils Pro

### ? À Faire Tous les Jours

```powershell
# Matin
.\start-both.ps1

# Avant de commit
.\check-sync.ps1

# Soir
.\stop-both.ps1
```

### ? À Ne Plus Faire

1. ? Démarrer manuellement dans 3 terminaux
2. ? Modifier `.env.local` à la main
3. ? Oublier de démarrer PostgreSQL
4. ? Changer les ports sans vérifier

---

## ?? Aide Rapide

### Erreur : "Port 5153 already in use"

```powershell
# Arrêter tout proprement
.\stop-both.ps1

# Redémarrer
.\start-both.ps1
```

### Erreur : "Docker not found"

1. Démarrer Docker Desktop manuellement
2. Attendre qu'il soit prêt (icône verte)
3. Relancer `.\start-both.ps1`

### Erreur : "PowerShell Execution Policy"

```powershell
# Autoriser l'exécution des scripts (une seule fois)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

---

## ?? Documentation

| Document | Contenu |
|----------|---------|
| **[GUIDE-SYNCHRONISATION.md](GUIDE-SYNCHRONISATION.md)** | Guide complet avec explications détaillées |
| **[SCRIPTS-REFERENCE.md](SCRIPTS-REFERENCE.md)** | Référence de tous les scripts |
| **[RAPPORT-SYNCHRONISATION-COMPLET.md](RAPPORT-SYNCHRONISATION-COMPLET.md)** | Analyse technique |

---

## ?? Résumé

### Ce Que Vous Devez Retenir

1. **Un seul script pour tout démarrer**
   ```powershell
   .\start-both.ps1
   ```

2. **Un seul script pour tout arrêter**
   ```powershell
   .\stop-both.ps1
   ```

3. **Un script pour vérifier**
   ```powershell
   .\check-sync.ps1
   ```

4. **Un script pour corriger**
   ```powershell
   .\fix-sync.ps1
   ```

### Plus Besoin de :
- ? Gérer 3 terminaux
- ? Se souvenir des ports
- ? Vérifier la configuration
- ? Ouvrir manuellement le navigateur

### Plus d'Erreur "Failed to Fetch" ! ??

---

**Créé le :** 3 février 2025  
**Objectif :** Résoudre définitivement les problèmes de synchronisation  
**Statut :** ? Opérationnel
