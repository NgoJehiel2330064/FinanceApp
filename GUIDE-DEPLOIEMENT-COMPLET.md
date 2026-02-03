# 🚀 GUIDE COMPLET : Déployer sur Vercel + Railway

## ✅ PRÉPARATION COMPLÈTE (Déjà fait!)

Tous les fichiers nécessaires sont prêts :
- ✅ `Dockerfile` pour Railway
- ✅ `vercel.json` pour Vercel  
- ✅ CORS configuré pour production
- ✅ `.dockerignore` et `.gitignore`

---

## 📋 ÉTAPE 1 : Installer Git (Si pas encore fait)

1. Téléchargez Git : https://git-scm.com/download/win
2. Installez avec les options par défaut
3. Redémarrez votre terminal

---

## 📦 ÉTAPE 2 : Créer le repo GitHub

### A. Sur GitHub.com :

1. Allez sur https://github.com
2. Cliquez sur le **+** en haut à droite → **"New repository"**
3. Configuration :
   - **Repository name** : `FinanceApp` (ou le nom de votre choix)
   - **Visibility** : Public ou Private (les deux fonctionnent)
   - ⚠️ **NE COCHEZ PAS** : "Add a README", ".gitignore", ou "licence"
4. Cliquez **"Create repository"**

### B. Dans votre terminal PowerShell :

```powershell
# Aller dans le dossier du projet
cd C:\Users\GOAT\OneDrive\Documents\FinanceApp

# Initialiser Git
git init

# Ajouter tous les fichiers
git add .

# Premier commit
git commit -m "Initial commit - FinanceApp ready for deployment"

# Créer la branche main
git branch -M main

# Ajouter le remote (REMPLACEZ 'VOTRE_USERNAME' par votre nom d'utilisateur GitHub)
git remote add origin https://github.com/VOTRE_USERNAME/FinanceApp.git

# Pousser le code
git push -u origin main
```

**Note :** GitHub vous demandera de vous connecter la première fois.

---

## 🚂 ÉTAPE 3 : Déployer le BACKEND sur Railway

### A. Créer un compte et un projet :

1. Allez sur https://railway.app
2. Cliquez **"Start a New Project"** (ou "Login with GitHub")
3. Cliquez **"Deploy from GitHub repo"**
4. Autorisez Railway à accéder à vos repos GitHub
5. Sélectionnez votre repo **"FinanceApp"**
6. Railway détectera automatiquement le `Dockerfile` ✅

### B. Ajouter PostgreSQL :

1. Dans votre projet Railway, cliquez sur **"+ New"**
2. Sélectionnez **"Database"** → **"Add PostgreSQL"**
3. Railway créera automatiquement la base de données
4. La variable `DATABASE_URL` sera générée automatiquement

### C. Configurer les variables d'environnement :

1. Cliquez sur votre service (le container avec le Dockerfile)
2. Allez dans l'onglet **"Variables"**
3. Cliquez **"+ New Variable"** et ajoutez **UNE PAR UNE** :

```
ConnectionStrings__DefaultConnection
→ Valeur : ${{Postgres.DATABASE_URL}}

JwtSettings__SecretKey
→ Valeur : Y0uR_sUp3r_s3cr3t_jwt_k3y_2025_F1N@nc3@pp!

JwtSettings__Issuer
→ Valeur : FinanceApp

JwtSettings__Audience
→ Valeur : FinanceAppUsers

ASPNETCORE_ENVIRONMENT
→ Valeur : Production

Groq__ApiKey
→ Valeur : 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
```

### D. Obtenir l'URL de votre API :

1. Allez dans l'onglet **"Settings"** de votre service
2. Dans **"Networking"**, cliquez sur **"Generate Domain"**
3. Railway vous donnera une URL comme : `https://financeapp-production-xxxx.up.railway.app`
4. **✍️ NOTEZ CETTE URL** - vous en aurez besoin pour Vercel !

### E. Vérifier le déploiement :

1. Attendez que le déploiement se termine (2-3 minutes)
2. Allez dans l'onglet **"Deployments"** pour voir les logs
3. Testez votre API : Ouvrez `https://VOTRE-URL-RAILWAY/swagger` dans un navigateur
4. Vous devriez voir la documentation Swagger ✅

---

## ▲ ÉTAPE 4 : Déployer le FRONTEND sur Vercel

### A. Créer un compte et importer le projet :

1. Allez sur https://vercel.com
2. Cliquez **"Add New..."** → **"Project"**
3. Cliquez **"Import Git Repository"**
4. Si ce n'est pas déjà fait, connectez votre compte GitHub
5. Trouvez et sélectionnez votre repo **"FinanceApp"**

### B. Configurer le projet :

Dans la page de configuration :

1. **Framework Preset** : Next.js (détecté automatiquement) ✅
2. **Root Directory** : Cliquez sur **"Edit"** et sélectionnez `finance-ui`
3. **Build Command** : `npm run build` (automatique)
4. **Output Directory** : `.next` (automatique)

### C. Configurer les variables d'environnement :

1. Dans la section **"Environment Variables"**, ajoutez :

```
NEXT_PUBLIC_API_URL
→ Valeur : https://VOTRE-URL-RAILWAY.up.railway.app
(⚠️ Remplacez par l'URL de Railway de l'étape 3D)

NEXT_PUBLIC_GEMINI_API_KEY
→ Valeur : AIzaSyCpYUPvjgvhPNtCjlJDg0ddmwCXPvUZRCg
```

2. Assurez-vous de sélectionner **"Production"**, **"Preview"**, et **"Development"**

### D. Déployer :

1. Cliquez **"Deploy"**
2. Attendez 2-3 minutes
3. Vercel vous donnera une URL comme : `https://finance-app-xyz.vercel.app`
4. **✍️ NOTEZ CETTE URL** - c'est l'URL de votre application !

---

## 🔧 ÉTAPE 5 : Configurer CORS (Important!)

Le backend doit autoriser les requêtes de votre frontend Vercel.

1. Retournez sur **Railway.app**
2. Cliquez sur votre service backend
3. Allez dans **"Variables"**
4. Ajoutez cette variable :

```
AllowedOrigins__0
→ Valeur : https://finance-app-xyz.vercel.app
(⚠️ Remplacez par VOTRE URL Vercel de l'étape 4D)
```

5. Le service redémarrera automatiquement

---

## 🎉 ÉTAPE 6 : TESTER !

### Testez votre application :

1. Ouvrez l'URL Vercel dans votre navigateur : `https://finance-app-xyz.vercel.app`
2. Créez un compte
3. Ajoutez des transactions
4. Testez toutes les fonctionnalités

### Partagez avec vos amis :

- Envoyez-leur simplement l'URL Vercel !
- Ils peuvent l'utiliser sur ordinateur, téléphone, tablette
- Disponible 24/7 partout dans le monde

---

## 📱 BONUS : Ajouter d'autres domaines autorisés

Si vous testez avec DevTunnels ou d'autres domaines :

1. Sur Railway, ajoutez plus de variables :
```
AllowedOrigins__1 = https://autre-domaine.com
AllowedOrigins__2 = https://encore-un-autre.com
```

---

## 🔄 METTRE À JOUR L'APPLICATION

Quand vous modifiez le code :

```powershell
# Aller dans le dossier
cd C:\Users\GOAT\OneDrive\Documents\FinanceApp

# Ajouter les modifications
git add .

# Commit
git commit -m "Description de vos changements"

# Pousser
git push
```

**Railway et Vercel redéploieront automatiquement !** 🎉

---

## 💰 COÛTS

### Vercel (Frontend) :
- ✅ **Gratuit** pour usage personnel
- 100 GB de bande passante / mois
- Builds illimités

### Railway (Backend) :
- ✅ **$5 de crédit gratuit** par mois
- Largement suffisant pour commencer
- Si vous dépassez : ~$0.01 par heure d'utilisation

---

## 🆘 DÉPANNAGE

### Erreur : "Load failed" sur mobile
→ Vérifiez que `NEXT_PUBLIC_API_URL` sur Vercel pointe vers Railway

### Erreur CORS
→ Vérifiez que `AllowedOrigins__0` sur Railway contient votre URL Vercel

### Backend ne démarre pas
→ Vérifiez les logs dans Railway (onglet "Deployments")
→ Assurez-vous que PostgreSQL est bien connecté

### Frontend affiche une erreur 500
→ Vérifiez que le backend Railway est en ligne
→ Testez l'API : `https://VOTRE-URL-RAILWAY/swagger`

---

## ✅ CHECKLIST FINALE

- [ ] Code pushé sur GitHub
- [ ] Backend déployé sur Railway
- [ ] PostgreSQL ajouté sur Railway
- [ ] Variables d'environnement configurées sur Railway
- [ ] URL Railway notée
- [ ] Frontend déployé sur Vercel
- [ ] Variables d'environnement configurées sur Vercel
- [ ] CORS configuré avec URL Vercel
- [ ] Application testée et fonctionnelle
- [ ] URL partagée avec vos amis

---

**🎊 Félicitations ! Votre application est maintenant accessible partout dans le monde !**
