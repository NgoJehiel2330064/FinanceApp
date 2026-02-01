# 🚀 Finance Dashboard - Configuration

## 📋 Configuration de l'API Gemini

Votre clé API Gemini est déjà configurée dans les deux emplacements :

### Backend C# (.NET)
✅ Fichier : `FinanceApp/appsettings.json`
```json
"Gemini": {
  "ApiKey": "AIzaSyCpYUPvjgvhPNtCjlJDg0ddmwCXPvUZRCg"
}
```

### Frontend Next.js
✅ Fichier : `finance-ui/.env.local`
```env
NEXT_PUBLIC_API_URL=https://localhost:7219
```

## 🔧 Lancement de l'application

### 1. Démarrer la base de données PostgreSQL
```bash
docker-compose up -d
```

### 2. Lancer l'API C#
```bash
cd FinanceApp
dotnet run
```
L'API sera disponible sur : `https://localhost:7219`

### 3. Lancer le Frontend Next.js
```bash
cd finance-ui
npm run dev
```
Le dashboard sera disponible sur : `http://localhost:3000`

## 🤖 Fonctionnalités IA activées

✅ **Conseil Financier** : `/api/finance/advice`
- Analyse automatique de vos transactions
- Conseils personnalisés générés par Gemini
- Affichage avec effet scintillement doré

✅ **Synchronisation en temps réel**
- Ajout de transactions via formulaire modal
- Mise à jour instantanée des statistiques
- Mode déconnecté élégant

## 🎨 Fonctionnalités Premium

- 🌙 Message de motivation selon l'heure
- 🎭 Design Glassmorphism
- ✨ Animations CSS fluides
- 📊 Calculs automatiques (Revenus, Dépenses, Solde)
- 🔒 Gestion d'erreurs robuste

## 🔐 Sécurité

⚠️ **Important** : Le fichier `.env.local` est dans `.gitignore` pour protéger vos clés API.
Ne commitez JAMAIS vos clés API sur GitHub !

## 📝 Structure du projet

```
FinanceApp/
├── FinanceApp/              # Backend C# (.NET 8)
│   ├── Controllers/         # API Controllers
│   ├── Services/           # Service Gemini
│   ├── Models/             # Modèles de données
│   └── appsettings.json    # Configuration
│
└── finance-ui/             # Frontend Next.js
    ├── app/                # Pages et layouts
    ├── lib/                # Configuration API
    └── .env.local          # Variables d'environnement
```

## 🆘 Dépannage

### L'API ne répond pas
1. Vérifiez que l'API C# est lancée : `dotnet run`
2. Vérifiez le port dans `.env.local`
3. Vérifiez les CORS dans `Program.cs`

### Les conseils IA ne s'affichent pas
1. Vérifiez la clé API Gemini dans `appsettings.json`
2. Ajoutez au moins une transaction
3. Vérifiez les logs du backend C#

### Erreurs de compilation Next.js
```bash
cd finance-ui
rm -rf .next node_modules
npm install
npm run dev
```

## 📊 API Endpoints disponibles

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/transactions` | Liste toutes les transactions |
| POST | `/api/transactions` | Ajoute une transaction |
| GET | `/api/finance/advice` | Conseil financier IA |
| GET | `/api/transactions/categories` | Liste des catégories |

---

✨ **Votre dashboard est prêt à l'emploi !**
