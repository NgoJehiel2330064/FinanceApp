# ?? FinanceApp - Application de Gestion Financière avec IA

Application ASP.NET Core 8 de gestion financière intégrant l'IA Google Gemini pour fournir des conseils financiers personnalisés.

---

## ?? Fonctionnalités

- ? **Gestion des transactions** : Créer, lire, mettre à jour et supprimer des transactions
- ? **Catégorisation** : Organiser vos transactions par catégorie
- ? **Calcul automatique du solde** : Revenus - Dépenses
- ? **Conseils financiers IA** : Recommandations personnalisées générées par Google Gemini
- ? **API RESTful** : Architecture moderne et scalable
- ? **Base de données PostgreSQL** : Stockage fiable avec Docker

---

## ??? Technologies Utilisées

- **Backend** : ASP.NET Core 8 (C# 12)
- **Base de données** : PostgreSQL 16
- **ORM** : Entity Framework Core 9
- **IA** : Google Gemini API (gemini-1.5-flash)
- **Conteneurisation** : Docker & Docker Compose
- **Documentation API** : Swagger/OpenAPI

---

## ?? Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Clé API Google Gemini](https://makersuite.google.com/app/apikey) (gratuite)

---

## ?? Installation

### 1?? Cloner le projet

```bash
git clone https://github.com/votre-username/FinanceApp.git
cd FinanceApp
```

### 2?? Démarrer PostgreSQL avec Docker

```bash
docker-compose up -d
```

Cela démarre un conteneur PostgreSQL sur `localhost:5432`.

### 3?? Configurer la clé API Gemini

**?? IMPORTANT** : Ne jamais committer votre clé API !

```bash
cd FinanceApp
dotnet user-secrets set "Gemini:ApiKey" "VOTRE_CLE_API_GEMINI"
```

Pour obtenir une clé gratuite : [Google AI Studio](https://makersuite.google.com/app/apikey)

### 4?? Appliquer les migrations

```bash
cd FinanceApp
dotnet ef database update
```

### 5?? Lancer l'application

**Option A : Script automatique (Recommandé)** ??

```bash
# Windows PowerShell
.\start-app.ps1

# Le script va :
# ? Vérifier et démarrer PostgreSQL
# ? Vérifier le port 5152
# ? Appliquer les migrations
# ? Lancer l'API
```

**Option B : Manuellement**

```bash
cd FinanceApp
dotnet run
```

### 6?? Arrêter l'application

```bash
# Avec le script
.\stop-app.ps1

# Manuellement
# Ctrl+C dans le terminal de l'API
# docker-compose down
```

L'application sera disponible sur :
- **HTTP** : http://localhost:5152
- **HTTPS** : https://localhost:7219
- **Swagger** : http://localhost:5152/swagger

---

## ?? API Endpoints

### Transactions

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| `GET` | `/api/transactions` | Liste toutes les transactions |
| `GET` | `/api/transactions/{id}` | Détails d'une transaction |
| `POST` | `/api/transactions` | Créer une transaction |
| `PUT` | `/api/transactions/{id}` | Modifier une transaction |
| `DELETE` | `/api/transactions/{id}` | Supprimer une transaction |
| `GET` | `/api/transactions/balance` | Calculer le solde total |

### Conseils Financiers (IA)

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| `GET` | `/api/finance/advice` | Obtenir un conseil financier généré par Gemini |

---

## ?? Exemples d'utilisation

### Créer une transaction

```http
POST /api/transactions
Content-Type: application/json

{
  "description": "Salaire mensuel",
  "amount": 3000,
  "type": "Income",
  "category": "Salaire",
  "date": "2025-02-01T00:00:00Z"
}
```

### Obtenir un conseil financier

```http
GET /api/finance/advice
```

Réponse :
```json
{
  "advice": "Réduisez vos dépenses en Alimentation de 15% pour économiser 200€ par mois."
}
```

---

## ??? Structure du Projet

```
FinanceApp/
??? FinanceApp/
?   ??? Controllers/           # API Controllers
?   ?   ??? TransactionsController.cs
?   ?   ??? FinanceController.cs
?   ??? Data/                  # DbContext
?   ?   ??? ApplicationDbContext.cs
?   ??? Models/                # Entités
?   ?   ??? Transaction.cs
?   ?   ??? Asset.cs
?   ??? Services/              # Services métier
?   ?   ??? IGeminiService.cs
?   ?   ??? GeminiService.cs
?   ??? Migrations/            # Migrations EF Core
?   ??? Program.cs             # Point d'entrée
?   ??? appsettings.json       # Configuration (SANS secrets)
??? docker-compose.yml         # Configuration Docker
??? .gitignore                 # Fichiers à ignorer
??? SECRETS-CONFIGURATION.md   # Guide de configuration des secrets
??? README.md                  # Ce fichier
```

---

## ?? Sécurité

- ? **User Secrets** : Clés API stockées en dehors du projet
- ? **`.gitignore`** : Empêche le commit de fichiers sensibles
- ? **Variables d'environnement** : Support pour la production
- ? **HTTPS** : Communication sécurisée activée

Voir [SECRETS-CONFIGURATION.md](./SECRETS-CONFIGURATION.md) pour plus de détails.

---

## ?? Docker

### Démarrer PostgreSQL

```bash
docker-compose up -d
```

### Arrêter PostgreSQL

```bash
docker-compose down
```

### Supprimer les données (?? Attention : destructif)

```bash
docker-compose down -v
```

---

## ?? Commandes Utiles

### Migrations

```bash
# Créer une nouvelle migration
dotnet ef migrations add NomDeLaMigration

# Appliquer les migrations
dotnet ef database update

# Supprimer la dernière migration
dotnet ef migrations remove
```

### User Secrets

```bash
# Lister les secrets
dotnet user-secrets list

# Ajouter un secret
dotnet user-secrets set "Cle:Valeur" "valeur"

# Supprimer un secret
dotnet user-secrets remove "Cle:Valeur"

# Effacer tous les secrets
dotnet user-secrets clear
```

---

## ?? Documentation

- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Google Gemini API](https://ai.google.dev/docs)
- [PostgreSQL](https://www.postgresql.org/docs/)

---

## ?? Documentation du projet

- **[README.md](README.md)** - Ce fichier (documentation principale)
- **[SECRETS-CONFIGURATION.md](SECRETS-CONFIGURATION.md)** - Configuration des clés API et secrets
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** - Guide de dépannage des problèmes courants
- **[FRONTEND-CONFIGURATION.md](FRONTEND-CONFIGURATION.md)** - Configuration du frontend (Next.js, React, etc.)

---

## ?? Contribution

Les contributions sont les bienvenues ! Veuillez :

1. Fork le projet
2. Créer une branche feature (`git checkout -b feature/AmazingFeature`)
3. Commit vos changements (`git commit -m 'Add some AmazingFeature'`)
4. Push vers la branche (`git push origin feature/AmazingFeature`)
5. Ouvrir une Pull Request

---

## ?? Licence

Ce projet est sous licence MIT. Voir le fichier [LICENSE](LICENSE) pour plus de détails.

---

## ????? Auteur

Votre Nom - [@votre_twitter](https://twitter.com/votre_twitter)

---

## ?? Roadmap

- [ ] Authentification JWT
- [ ] Gestion multi-utilisateurs
- [ ] Dashboard avec graphiques
- [ ] Export PDF des rapports
- [ ] Application mobile (Xamarin/MAUI)
- [ ] Notifications par email
- [ ] Import de fichiers CSV

---

## ?? Fonctionnalités IA Futures

- [ ] Suggestion automatique de catégorie (via Gemini)
- [ ] Détection d'anomalies dans les dépenses
- [ ] Prédiction de budget pour les prochains mois
- [ ] Résumé financier mensuel automatique
- [ ] Conseils personnalisés basés sur l'historique

---

## ?? Support

Si vous rencontrez des problèmes :

1. Consultez [SECRETS-CONFIGURATION.md](./SECRETS-CONFIGURATION.md)
2. Vérifiez les [Issues](https://github.com/votre-username/FinanceApp/issues)
3. Créez une nouvelle issue si nécessaire

---

**? Si ce projet vous a aidé, n'hésitez pas à lui donner une étoile sur GitHub !**
