# Guide de déploiement pour rendre l'application accessible partout

## Option 1 : Déploiement Cloud (Recommandé) 🌐

### Frontend sur Vercel (Gratuit)
1. Créez un compte sur https://vercel.com
2. Installez Vercel CLI :
   ```powershell
   npm install -g vercel
   ```
3. Dans le dossier finance-ui :
   ```powershell
   cd finance-ui
   vercel login
   vercel
   ```
4. Suivez les instructions (appuyez sur Entrée pour accepter les valeurs par défaut)
5. Vercel vous donnera une URL publique (ex: https://finance-app-xyz.vercel.app)

### Backend sur Railway (Gratuit)
1. Créez un compte sur https://railway.app
2. Cliquez sur "New Project" → "Deploy from GitHub repo"
3. Connectez votre repo GitHub (ou créez-en un)
4. Railway détectera automatiquement le Dockerfile
5. Ajoutez une base de données PostgreSQL dans Railway
6. Configurez les variables d'environnement :
   - `ConnectionStrings__DefaultConnection` = (fournie par Railway)
   - `JwtSettings__SecretKey` = (copiez depuis appsettings.json)
7. Railway vous donnera une URL publique (ex: https://financeapp-production.up.railway.app)

### Mise à jour de la configuration
Une fois déployé, mettez à jour le .env.local avec l'URL Railway :
```
NEXT_PUBLIC_API_URL=https://votre-app-railway.up.railway.app
```

## Option 2 : ngrok (Solution temporaire rapide) 🚀

### Installation
1. Téléchargez ngrok : https://ngrok.com/download
2. Créez un compte gratuit sur ngrok.com
3. Installez ngrok

### Utilisation
Dans deux terminaux séparés :

**Terminal 1 - Tunnel Backend :**
```powershell
ngrok http 5153
```
Notez l'URL (ex: https://abc123.ngrok.io)

**Terminal 2 - Tunnel Frontend :**
```powershell
ngrok http 3000
```
Notez l'URL (ex: https://def456.ngrok.io)

Mettez à jour .env.local avec l'URL du backend ngrok.

**Avantages :** Très rapide à mettre en place
**Inconvénients :** Les URLs changent à chaque redémarrage, nécessite que votre PC reste allumé

## Option 3 : Hébergement sur serveur personnel

Si vous avez un serveur ou un Raspberry Pi :
1. Configurez un nom de domaine
2. Configurez le port forwarding sur votre routeur (ports 80, 443)
3. Utilisez Nginx comme reverse proxy
4. Configurez SSL avec Let's Encrypt

## Recommandation 🎯

**Pour partager avec des amis de façon permanente :**
- Utilisez **Vercel (frontend)** + **Railway (backend)**
- C'est gratuit et disponible 24/7
- Les URLs ne changent jamais
- Configuration en 10 minutes

**Pour tester rapidement (1-2 heures) :**
- Utilisez **ngrok**
- Votre PC doit rester allumé

## Prochaines étapes

Dites-moi quelle option vous préférez et je vous guiderai dans le déploiement !
