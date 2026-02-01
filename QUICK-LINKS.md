# ?? FinanceApp - Liens Rapides

## ?? URLs de l'Application

### Développement Local

| Service | URL | Description |
|---------|-----|-------------|
| **API HTTP** | http://localhost:5152 | Endpoint principal |
| **API HTTPS** | https://localhost:7219 | Endpoint sécurisé |
| **Swagger UI** | http://localhost:5152/swagger | Documentation interactive ? |
| **PostgreSQL** | localhost:5432 | Base de données |

---

## ?? Documentation

| Document | Chemin | Utilité |
|----------|--------|---------|
| **Résumé Sprint 2** | `SUMMARY-SPRINT2.md` | Vue d'ensemble rapide ? |
| **Guide Tests Swagger** | `SWAGGER-TESTS-GUIDE.md` | Tests pas-à-pas ? |
| **Exemples API** | `API-EXAMPLES.md` | Code PowerShell/curl/JS |
| **Implémentation** | `SPRINT2-IMPLEMENTATION.md` | Détails techniques |
| **Cahier des charges** | `CAHIER-DES-CHARGES.md` | Référence complète |
| **Démarrage rapide** | `QUICK-START.md` | Installation |
| **Dépannage** | `TROUBLESHOOTING.md` | Résolution problèmes |
| **Sécurité** | `SECRETS-CONFIGURATION.md` | Gestion clés API |
| **Frontend** | `FRONTEND-CONFIGURATION.md` | Intégration Next.js |

---

## ?? Endpoints API (15 Total)

### Transactions (6)
```
GET    /api/transactions
GET    /api/transactions/{id}
POST   /api/transactions
PUT    /api/transactions/{id}
DELETE /api/transactions/{id}
GET    /api/transactions/balance
```

### Assets (6)
```
GET    /api/assets
GET    /api/assets/{id}
POST   /api/assets
PUT    /api/assets/{id}
DELETE /api/assets/{id}
GET    /api/assets/total-value
```

### Finance IA (5)
```
GET    /api/finance/advice
POST   /api/finance/suggest-category
GET    /api/finance/summary?startDate=...&endDate=...
GET    /api/finance/anomalies
GET    /api/finance/predict?monthsAhead=...
```

---

## ?? Commandes Rapides

### Démarrage
```powershell
.\start-app.ps1
```

### Test Swagger
```powershell
start http://localhost:5152/swagger
```

### Tests API
```powershell
# Conseil IA
Invoke-RestMethod -Uri "http://localhost:5152/api/finance/advice"

# Assets
Invoke-RestMethod -Uri "http://localhost:5152/api/assets"

# Solde
Invoke-RestMethod -Uri "http://localhost:5152/api/transactions/balance"
```

### Arrêt
```powershell
.\stop-app.ps1
```

### Vérification config
```powershell
.\test-config.ps1
```

---

## ?? User Secrets

### Voir la clé API
```powershell
cd FinanceApp
dotnet user-secrets list
```

### Modifier la clé
```powershell
dotnet user-secrets set "Gemini:ApiKey" "VOTRE_NOUVELLE_CLE"
```

---

## ?? Docker

### PostgreSQL
```powershell
# Démarrer
docker-compose up -d

# Arrêter
docker-compose down

# Logs
docker logs postgres_db

# Shell PostgreSQL
docker exec -it postgres_db psql -U postgres -d finance_db
```

---

## ?? Migrations

### Créer migration
```powershell
cd FinanceApp
dotnet ef migrations add NomMigration
```

### Appliquer migrations
```powershell
dotnet ef database update
```

### Voir migrations
```powershell
dotnet ef migrations list
```

---

## ?? Build & Run

### Build
```powershell
dotnet build
```

### Run (sans script)
```powershell
cd FinanceApp
dotnet run
```

### Watch (redémarrage auto)
```powershell
dotnet watch run
```

---

## ?? Métriques Projet

| Métrique | Valeur |
|----------|--------|
| **Backend** | 95% |
| **Endpoints** | 15/15 (100%) |
| **Controllers** | 3/3 (100%) |
| **Méthodes IA** | 5/5 (100%) |
| **Documentation** | 9 fichiers |

---

## ? Checklist de Validation

- [ ] PostgreSQL tourne (`docker ps`)
- [ ] Port 5152 libre (`netstat -ano | findstr :5152`)
- [ ] Clé API configurée (`dotnet user-secrets list`)
- [ ] API démarre (`.\start-app.ps1`)
- [ ] Swagger accessible (http://localhost:5152/swagger)
- [ ] Endpoints testés (voir `SWAGGER-TESTS-GUIDE.md`)

---

## ?? En cas de problème

1. **Consultez** `TROUBLESHOOTING.md`
2. **Vérifiez** les logs du terminal
3. **Testez** `.\test-config.ps1`
4. **Redémarrez** Docker Desktop
5. **Recréez** la base : `docker-compose down -v` puis `docker-compose up -d`

---

## ?? Ressources Externes

| Ressource | URL |
|-----------|-----|
| **ASP.NET Core** | https://learn.microsoft.com/aspnet/core |
| **Entity Framework** | https://learn.microsoft.com/ef/core |
| **PostgreSQL** | https://www.postgresql.org/docs/ |
| **Gemini API** | https://ai.google.dev/docs |
| **Swagger** | https://swagger.io/docs/ |

---

## ?? Prochaines Étapes

### Immédiatement
1. ? Lancer l'API : `.\start-app.ps1`
2. ? Tester Swagger : http://localhost:5152/swagger
3. ? Suivre `SWAGGER-TESTS-GUIDE.md`

### Sprint 3 (Frontend)
1. ? Créer projet Next.js
2. ? Implémenter Dashboard
3. ? Intégrer API

---

**Tout est prêt pour le développement ! ??**

**Document mis à jour :** Février 2025
