# ??? Guide : Raccourcis Bureau pour FinanceApp

## ?? Objectif

Créer des raccourcis sur votre **Bureau Windows** pour démarrer/arrêter FinanceApp d'un simple **double-clic**, sans avoir à ouvrir PowerShell ou VS Code.

---

## ? Félicitations ! Synchronisation Réussie

Avant de continuer, bravo pour avoir résolu l'erreur "Failed to Fetch" ! ??

Maintenant, simplifions encore plus votre workflow.

---

## ?? Installation Automatique

### **Étape 1 : Ouvrir PowerShell dans VS Code**

1. Appuyez sur `Ctrl + ù` (ou `Ctrl + ²`)
2. Assurez-vous d'être dans le dossier du projet :
```powershell
Get-Location
# Doit afficher : C:\Users\GOAT\OneDrive\Documents\FinanceApp
```

### **Étape 2 : Exécuter le Script d'Installation**

```powershell
.\Install-Desktop-Shortcuts.ps1
```

**Ce qui se passe :**
- ? 3 scripts sont créés dans le dossier du projet
- ? 3 raccourcis sont créés sur votre Bureau
- ? Les icônes Windows sont automatiquement assignées

### **Étape 3 : Vérifier le Bureau**

Allez sur votre Bureau Windows. Vous devriez voir :

```
??? Bureau Windows
??? ?? Démarrer FinanceApp.lnk
??? ?? Arrêter FinanceApp.lnk
??? ?? Vérifier FinanceApp.lnk
```

---

## ?? Utilisation des Raccourcis

### **?? Démarrer FinanceApp**

**Double-cliquez sur le raccourci "?? Démarrer FinanceApp"**

**Ce qui se passe :**
1. Une fenêtre PowerShell s'ouvre
2. PostgreSQL démarre automatiquement
3. Backend démarre (nouvelle fenêtre)
4. Frontend démarre (nouvelle fenêtre)
5. Votre navigateur s'ouvre sur http://localhost:3000

**Durée :** ~15 secondes

---

### **?? Arrêter FinanceApp**

**Double-cliquez sur le raccourci "?? Arrêter FinanceApp"**

**Ce qui se passe :**
1. Une fenêtre PowerShell s'ouvre
2. Frontend arrêté (port 3000)
3. Backend arrêté (port 5153)
4. PostgreSQL arrêté
5. Toutes les fenêtres fermées

**Durée :** ~3 secondes

---

### **?? Vérifier FinanceApp**

**Double-cliquez sur le raccourci "?? Vérifier FinanceApp"**

**Ce qui se passe :**
1. Une fenêtre PowerShell s'ouvre
2. Vérification de la synchronisation backend/frontend
3. Affichage du statut de tous les services
4. Conseils si des problèmes sont détectés

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

## ?? Workflow Quotidien Simplifié

### **Avant (Manuel)**

```powershell
# Terminal 1
cd C:\Users\GOAT\OneDrive\Documents\FinanceApp
docker-compose up -d

# Terminal 2
cd C:\Users\GOAT\OneDrive\Documents\FinanceApp\FinanceApp
dotnet run

# Terminal 3
cd C:\Users\GOAT\OneDrive\Documents\FinanceApp\finance-ui
npm run dev
```

**Problème :** 3 terminaux, 6 commandes, 2 minutes

---

### **Après (Raccourcis Bureau)**

```
1. Double-clic sur ?? Démarrer FinanceApp
2. Attendre 15 secondes
3. Coder !
4. Double-clic sur ?? Arrêter FinanceApp
```

**Avantage :** 2 double-clics, 15 secondes ! ??

---

## ?? Personnalisation

### **Changer le Chemin du Projet**

Si vous déplacez le projet FinanceApp, vous devez mettre à jour les scripts :

1. **Ouvrir `Desktop-Start-FinanceApp.ps1`**
2. **Modifier la ligne 3 :**
```powershell
$projectPath = "C:\Users\GOAT\OneDrive\Documents\FinanceApp"
```
3. **Remplacer par le nouveau chemin**
4. **Enregistrer**
5. **Répéter pour les autres scripts bureau**

**Alternative :** Relancer `.\Install-Desktop-Shortcuts.ps1` après le déplacement.

---

### **Changer les Icônes**

Les icônes sont définies dans `Install-Desktop-Shortcuts.ps1` :

```powershell
$shortcuts = @(
    @{
        Name = "?? Démarrer FinanceApp.lnk"
        Icon = "imageres.dll,1"      # ?? Icône verte
    },
    @{
        Name = "?? Arrêter FinanceApp.lnk"
        Icon = "imageres.dll,84"     # ?? Icône rouge
    },
    @{
        Name = "?? Vérifier FinanceApp.lnk"
        Icon = "imageres.dll,76"     # ?? Icône loupe
    }
)
```

**Icônes Windows disponibles :**
- `imageres.dll,1` ? Dossier vert
- `imageres.dll,84` ? Croix rouge
- `imageres.dll,76` ? Loupe
- `imageres.dll,3` ? Ordinateur
- `imageres.dll,27` ? Engrenage
- `imageres.dll,109` ? Fusée

**Voir toutes les icônes :** `C:\Windows\System32\imageres.dll`

---

## ?? Comparaison : Avant/Après

| Tâche | Avant | Après |
|-------|-------|-------|
| **Démarrer FinanceApp** | 6 commandes, 3 terminaux | 1 double-clic |
| **Arrêter FinanceApp** | Ctrl+C × 3, docker stop | 1 double-clic |
| **Vérifier Sync** | Commande manuelle | 1 double-clic |
| **Temps de démarrage** | ~2 minutes | ~15 secondes |
| **Complexité** | ???? | ? |

---

## ?? Comment Ça Marche ?

### **Architecture des Raccourcis**

```
Bureau Windows
??? ?? Démarrer FinanceApp.lnk
?   ?
?   Appelle : Desktop-Start-FinanceApp.ps1
?   ?
?   Navigue vers : C:\Users\GOAT\OneDrive\Documents\FinanceApp
?   ?
?   Exécute : start-both.ps1
?   ?
?   Résultat : Backend + Frontend démarrés
?
??? ?? Arrêter FinanceApp.lnk
?   ?
?   Appelle : Desktop-Stop-FinanceApp.ps1
?   ?
?   Exécute : stop-both.ps1
?   ?
?   Résultat : Tout arrêté
?
??? ?? Vérifier FinanceApp.lnk
    ?
    Appelle : Desktop-Check-FinanceApp.ps1
    ?
    Exécute : check-sync.ps1
    ?
    Résultat : Statut affiché
```

### **Pourquoi Ça Fonctionne de Partout ?**

Les scripts bureau contiennent le **chemin absolu** du projet :

```powershell
$projectPath = "C:\Users\GOAT\OneDrive\Documents\FinanceApp"
Set-Location $projectPath
```

Donc peu importe d'où vous double-cliquez, le script navigue automatiquement vers le bon dossier.

---

## ?? Dépannage

### **Erreur : "Impossible d'exécuter le script"**

**Symptôme :**
```
L'exécution de scripts est désactivée sur ce système
```

**Solution :**
1. Ouvrir PowerShell en **Administrateur**
2. Exécuter :
```powershell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```
3. Confirmer avec `O` (Oui)
4. Fermer PowerShell
5. Relancer le raccourci

---

### **Erreur : "Dossier introuvable"**

**Symptôme :**
```
? Dossier introuvable : C:\Users\GOAT\OneDrive\Documents\FinanceApp
```

**Solution :**
1. Ouvrir `Desktop-Start-FinanceApp.ps1` avec un éditeur de texte
2. Modifier la ligne 3 avec le bon chemin
3. Enregistrer
4. Relancer `.\Install-Desktop-Shortcuts.ps1`

---

### **Les Raccourcis ne se Créent Pas**

**Solution :**
```powershell
# Réinstaller les raccourcis
cd C:\Users\GOAT\OneDrive\Documents\FinanceApp
.\Install-Desktop-Shortcuts.ps1
```

---

### **Double-Clic ne Fait Rien**

**Solution :**
1. Clic droit sur le raccourci ? **Propriétés**
2. Vérifier que **Cible** contient :
```
powershell.exe -ExecutionPolicy Bypass -NoProfile -File "C:\Users\GOAT\OneDrive\Documents\FinanceApp\Desktop-Start-FinanceApp.ps1"
```
3. Si incorrect, réinstaller avec `.\Install-Desktop-Shortcuts.ps1`

---

## ?? Conseils Pro

### **Créer un Dossier "FinanceApp" sur le Bureau**

Au lieu d'avoir 3 raccourcis éparpillés :

1. Créer un dossier `?? FinanceApp` sur le Bureau
2. Y déplacer les 3 raccourcis
3. Épingler le dossier à la barre des tâches

**Résultat :** Accès rapide et Bureau ordonné !

---

### **Ajouter un Raccourci "Ouvrir VS Code"**

Vous pouvez créer un 4ème raccourci pour ouvrir directement VS Code :

1. Clic droit sur Bureau ? **Nouveau** ? **Raccourci**
2. Cible :
```
code "C:\Users\GOAT\OneDrive\Documents\FinanceApp"
```
3. Nom : `?? Ouvrir FinanceApp dans VS Code`

---

## ?? Fichiers Créés

| Fichier | Emplacement | Fonction |
|---------|-------------|----------|
| **Install-Desktop-Shortcuts.ps1** | Racine projet | Script d'installation |
| **Desktop-Start-FinanceApp.ps1** | Racine projet | Script bureau démarrage |
| **Desktop-Stop-FinanceApp.ps1** | Racine projet | Script bureau arrêt |
| **Desktop-Check-FinanceApp.ps1** | Racine projet | Script bureau vérification |
| **?? Démarrer FinanceApp.lnk** | Bureau Windows | Raccourci démarrage |
| **?? Arrêter FinanceApp.lnk** | Bureau Windows | Raccourci arrêt |
| **?? Vérifier FinanceApp.lnk** | Bureau Windows | Raccourci vérification |

---

## ?? Résumé

### **Pour Installer les Raccourcis**

```powershell
# 1. Ouvrir PowerShell dans VS Code
# 2. Naviguer vers le dossier du projet
cd C:\Users\GOAT\OneDrive\Documents\FinanceApp

# 3. Exécuter le script d'installation
.\Install-Desktop-Shortcuts.ps1
```

### **Pour Utiliser les Raccourcis**

1. **Double-clic sur ?? Démarrer FinanceApp**
2. Attendre 15 secondes
3. Coder !
4. **Double-clic sur ?? Arrêter FinanceApp**

**C'est tout !** ??

---

## ? Checklist Finale

- [ ] Exécuter `.\Install-Desktop-Shortcuts.ps1`
- [ ] Vérifier les 3 raccourcis sur le Bureau
- [ ] Tester **?? Démarrer FinanceApp**
- [ ] Attendre l'ouverture du navigateur
- [ ] Vérifier que tout fonctionne
- [ ] Tester **?? Arrêter FinanceApp**
- [ ] Tester **?? Vérifier FinanceApp**

---

**Besoin d'aide ?** Consultez les autres guides :
- **[GUIDE-SYNCHRONISATION.md](GUIDE-SYNCHRONISATION.md)** - Guide complet
- **[SCRIPTS-REFERENCE.md](SCRIPTS-REFERENCE.md)** - Référence des scripts
- **[SOLUTION-FINALE-FAILED-TO-FETCH.md](SOLUTION-FINALE-FAILED-TO-FETCH.md)** - Solution résumée

---

**Créé le :** 3 février 2025  
**Version :** 1.0  
**Objectif :** Simplifier le démarrage de FinanceApp avec des raccourcis bureau
