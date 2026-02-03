# ?? Scripts de D�marrage FinanceApp

Ce dossier contient **tous les scripts n�cessaires** pour d�marrer et g�rer FinanceApp facilement.

---

## ?? Scripts Disponibles

### **?? Scripts Principaux (� Utiliser dans VS Code)**

| Script | Description | Usage |
|--------|-------------|-------|
| **start-both.ps1** | D�marre PostgreSQL + Backend + Frontend | `.\Scripts-Demarrage\start-both.ps1` |
| **stop-both.ps1** | Arr�te tous les services | `.\Scripts-Demarrage\stop-both.ps1` |
| **check-sync.ps1** | V�rifie la synchronisation backend/frontend | `.\Scripts-Demarrage\check-sync.ps1` |
| **fix-sync.ps1** | Corrige automatiquement la configuration | `.\Scripts-Demarrage\fix-sync.ps1` |

---

### **??? Scripts Bureau (Pour Double-Clic)**

| Script | Description |
|--------|-------------|
| **Desktop-Start-FinanceApp.ps1** | Script bureau pour d�marrer |
| **Desktop-Stop-FinanceApp.ps1** | Script bureau pour arr�ter |
| **Desktop-Check-FinanceApp.ps1** | Script bureau pour v�rifier |
| **Install-Desktop-Shortcuts.ps1** | Cr�e les raccourcis sur le bureau |

---

## ?? D�marrage Rapide

### **✨ Méthode 1 : Démarrage Ultra-Rapide (RECOMMANDÉ)**

```powershell
# Terminal PowerShell dans VS Code
.\Scripts-Demarrage\DEMARRER-RAPIDE.ps1
```

✅ Lance Backend + Frontend en 1 clique
✅ Corrige automatiquement les ports  
✅ Ouvre le navigateur sur http://localhost:3000

Pour arrêter rapidement :
```powershell
.\Scripts-Demarrage\ARRETER-RAPIDE.ps1
```

### **Méthode 2 : Depuis VS Code (Détaillée)**

```powershell
# Terminal PowerShell dans VS Code
.\Scripts-Demarrage\start-both.ps1
```

---

### **M�thode 2 : Depuis le Bureau (Recommand� pour Utilisation Quotidienne)**

#### **�tape 1 : Installer les Raccourcis (Une Seule Fois)**

```powershell
# Dans VS Code Terminal
.\Scripts-Demarrage\Install-Desktop-Shortcuts.ps1
```

#### **�tape 2 : Utiliser les Raccourcis**

Sur votre **Bureau Windows**, double-cliquez sur :

- **?? D�marrer FinanceApp** ? Tout d�marre automatiquement
- **?? Arr�ter FinanceApp** ? Tout s'arr�te proprement
- **?? V�rifier FinanceApp** ? Affiche le statut

---

## ?? Workflow Quotidien

### **Option A : D�veloppeur (VS Code)**

```powershell
# Matin
.\Scripts-Demarrage\check-sync.ps1
.\Scripts-Demarrage\start-both.ps1

# Coder...

# Soir
.\Scripts-Demarrage\stop-both.ps1
```

---

### **Option B : Utilisateur (Bureau)**

```
1. Double-clic sur ?? D�marrer FinanceApp
2. Attendre 15 secondes
3. Coder !
4. Double-clic sur ?? Arr�ter FinanceApp
```

---

## ?? R�solution de Probl�mes

### **Erreur "Failed to Fetch"**

```powershell
.\Scripts-Demarrage\stop-both.ps1
.\Scripts-Demarrage\fix-sync.ps1
.\Scripts-Demarrage\start-both.ps1
```

---

### **V�rifier la Synchronisation**

```powershell
.\Scripts-Demarrage\check-sync.ps1
```

**R�sultat attendu :**
```
? Backend : Port 5153 ?
? Frontend : URL API correcte ?
? CORS : localhost:3000 autoris� ?
? Backend : En cours d'ex�cution sur le port 5153 ?
? Frontend : En cours d'ex�cution sur le port 3000 ?
? PostgreSQL : En cours d'ex�cution ?

? CONFIGURATION PARFAITEMENT SYNCHRONIS�E
```

---

## ?? Structure du Dossier

```
Scripts-Demarrage/
??? README.md                          ? Vous �tes ici
??? start-both.ps1                     ? D�marrage complet
??? stop-both.ps1                      ? Arr�t complet
??? check-sync.ps1                     ? V�rification
??? fix-sync.ps1                       ? Correction
??? Desktop-Start-FinanceApp.ps1       ? Script bureau (d�marrage)
??? Desktop-Stop-FinanceApp.ps1        ? Script bureau (arr�t)
??? Desktop-Check-FinanceApp.ps1       ? Script bureau (v�rification)
??? Install-Desktop-Shortcuts.ps1      ? Installation raccourcis
```

---

## ?? Avantages de Ce Dossier

### ? **Organisation**
- Tous les scripts au m�me endroit
- Facile � trouver
- Nom significatif : **Scripts-Demarrage**

### ? **Simplicit�**
- Chemins courts : `.\Scripts-Demarrage\start-both.ps1`
- Pas de scripts �parpill�s � la racine

### ? **Flexibilit�**
- Utilisation depuis VS Code : `.\Scripts-Demarrage\...`
- Utilisation depuis le Bureau : Double-clic sur raccourci

---

## ?? Conseils Pro

### **�pingler le Dossier**

1. Ouvrir l'Explorateur Windows
2. Naviguer vers : `C:\Users\GOAT\OneDrive\Documents\FinanceApp\Scripts-Demarrage`
3. Clic droit ? **�pingler � Acc�s rapide**

**R�sultat :** Acc�s instantan� depuis n'importe o� !

---

### **Cr�er un Raccourci du Dossier sur le Bureau**

```powershell
# Depuis le dossier du projet
$desktop = [Environment]::GetFolderPath("Desktop")
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut("$desktop\?? Scripts FinanceApp.lnk")
$shortcut.TargetPath = "$PWD\Scripts-Demarrage"
$shortcut.IconLocation = "C:\Windows\System32\imageres.dll,3"
$shortcut.Save()
```

---

## ?? Documentation Compl�te

| Document | Description |
|----------|-------------|
| [GUIDE-SYNCHRONISATION.md](../GUIDE-SYNCHRONISATION.md) | Guide complet de synchronisation |
| [SCRIPTS-REFERENCE.md](../SCRIPTS-REFERENCE.md) | R�f�rence de tous les scripts |
| [GUIDE-RACCOURCIS-BUREAU.md](../GUIDE-RACCOURCIS-BUREAU.md) | Guide des raccourcis bureau |
| [SOLUTION-FINALE-FAILED-TO-FETCH.md](../SOLUTION-FINALE-FAILED-TO-FETCH.md) | Solution � "Failed to Fetch" |

---

## ? R�sum�

### **Pour D�marrer**
```powershell
.\Scripts-Demarrage\start-both.ps1
```

### **Pour Arr�ter**
```powershell
.\Scripts-Demarrage\stop-both.ps1
```

### **Pour V�rifier**
```powershell
.\Scripts-Demarrage\check-sync.ps1
```

### **Pour Installer les Raccourcis Bureau**
```powershell
.\Scripts-Demarrage\Install-Desktop-Shortcuts.ps1
```

---

**C'est tout ! ??**

Tous vos scripts de d�marrage sont maintenant organis�s dans ce dossier unique et facile � trouver.
