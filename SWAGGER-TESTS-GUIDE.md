# ?? Guide de Tests Swagger - FinanceApp

Ce guide vous permet de tester rapidement tous les nouveaux endpoints implémentés dans le Sprint 2.

**Accès Swagger UI :** http://localhost:5152/swagger

---

## ?? Démarrage Rapide

### 1. Lancer l'application
```powershell
.\start-app.ps1
```

### 2. Ouvrir Swagger
```powershell
start http://localhost:5152/swagger
```

---

## ?? Tests Assets Controller

### ? Test 1 : Créer un actif immobilier

**Endpoint :** `POST /api/assets`

**Body JSON :**
```json
{
  "name": "Appartement Paris 15e",
  "value": 320000,
  "type": "RealEstate",
  "acquisitionDate": "2020-06-15T00:00:00Z"
}
```

**Résultat attendu :**
- Status : `201 Created`
- Body contient l'ID généré
- Header `Location` : `/api/assets/{id}`

---

### ? Test 2 : Créer un véhicule

**Endpoint :** `POST /api/assets`

**Body JSON :**
```json
{
  "name": "Renault Clio",
  "value": 12000,
  "type": "Vehicle",
  "acquisitionDate": "2022-03-10T00:00:00Z"
}
```

---

### ? Test 3 : Créer un investissement

**Endpoint :** `POST /api/assets`

**Body JSON :**
```json
{
  "name": "Actions Total",
  "value": 5000,
  "type": "Investment",
  "acquisitionDate": "2023-01-05T00:00:00Z"
}
```

---

### ? Test 4 : Lister tous les actifs

**Endpoint :** `GET /api/assets`

**Résultat attendu :**
- Status : `200 OK`
- Body : Tableau JSON avec 3 actifs

---

### ? Test 5 : Récupérer un actif par ID

**Endpoint :** `GET /api/assets/{id}`

**Paramètre :** Remplacez `{id}` par l'ID d'un actif créé (ex: 1)

**Résultat attendu :**
- Status : `200 OK`
- Body : Détails complets de l'actif

---

### ? Test 6 : Calculer la valeur totale du patrimoine

**Endpoint :** `GET /api/assets/total-value`

**Résultat attendu :**
- Status : `200 OK`
- Body : `337000` (320000 + 12000 + 5000)

---

### ? Test 7 : Modifier un actif

**Endpoint :** `PUT /api/assets/{id}`

**Body JSON :** (exemple avec ID 1)
```json
{
  "id": 1,
  "name": "Appartement Paris 15e (rénové)",
  "value": 350000,
  "type": "RealEstate",
  "acquisitionDate": "2020-06-15T00:00:00Z"
}
```

**Résultat attendu :**
- Status : `204 No Content`

**Vérification :**
- Re-tester `GET /api/assets/1` ? Valeur doit être 350000
- Re-tester `GET /api/assets/total-value` ? Nouveau total : 367000

---

### ? Test 8 : Supprimer un actif

**Endpoint :** `DELETE /api/assets/{id}`

**Paramètre :** ID d'un actif à supprimer (ex: 3)

**Résultat attendu :**
- Status : `204 No Content`

**Vérification :**
- Re-tester `GET /api/assets` ? Ne contient plus l'actif supprimé

---

## ?? Tests Finance Controller (IA)

### ? Test 9 : Suggestion de catégorie

**Endpoint :** `POST /api/finance/suggest-category`

**Body JSON :**
```json
{
  "description": "Netflix abonnement mensuel",
  "amount": 15.99
}
```

**Résultat attendu :**
- Status : `200 OK`
- Body : `{ "category": "Loisirs" }` ou `"Services"`

---

**Autres exemples à tester :**

```json
{
  "description": "Courses Lidl",
  "amount": 85.50
}
```
? Attendu : `"Alimentation"`

```json
{
  "description": "Essence Total",
  "amount": 60.00
}
```
? Attendu : `"Transport"`

```json
{
  "description": "Loyer appartement",
  "amount": 800.00
}
```
? Attendu : `"Logement"`

---

### ? Test 10 : Résumé financier

**Prérequis :** Avoir des transactions en janvier 2025

**Endpoint :** `GET /api/finance/summary`

**Query Parameters :**
- `startDate` : `2025-01-01`
- `endDate` : `2025-01-31`

**Résultat attendu :**
- Status : `200 OK`
- Body : `{ "summary": "En janvier, vous avez économisé 71% de vos revenus..." }`

---

**Si vous n'avez pas de transactions en janvier, créez-en quelques-unes :**

```bash
# Via Swagger : POST /api/transactions
{
  "description": "Salaire mensuel",
  "amount": 3000,
  "type": "Income",
  "category": "Salaire",
  "date": "2025-01-15T00:00:00Z"
}

{
  "description": "Courses",
  "amount": 450,
  "type": "Expense",
  "category": "Alimentation",
  "date": "2025-01-10T00:00:00Z"
}

{
  "description": "Loyer",
  "amount": 800,
  "type": "Expense",
  "category": "Logement",
  "date": "2025-01-01T00:00:00Z"
}
```

---

### ? Test 11 : Détection d'anomalies

**Endpoint :** `GET /api/finance/anomalies`

**Scénario test :** Créer une transaction inhabituelle

```json
POST /api/transactions
{
  "description": "Achat exceptionnel",
  "amount": 2000,
  "type": "Expense",
  "category": "Loisirs",
  "date": "2025-02-01T00:00:00Z"
}
```

**Ensuite tester :**
```
GET /api/finance/anomalies
```

**Résultat attendu :**
- Status : `200 OK`
- Body : 
```json
{
  "anomalies": [
    "Dépense inhabituelle : 2000€ en Loisirs le 01/02/2025 (moyenne: 50€, écart: +3900%)"
  ]
}
```

---

### ? Test 12 : Prédiction de budget

**Prérequis :** Au moins 3 mois d'historique de transactions

**Endpoint :** `GET /api/finance/predict`

**Query Parameters :**
- `monthsAhead` : `3` (prédire pour 3 mois)

**Résultat attendu :**
- Status : `200 OK`
- Body : `{ "prediction": "Avec vos économies mensuelles de 700€, vous devriez atteindre 2100€ dans 3 mois..." }`

---

**Si vous n'avez pas assez d'historique :**

Créez des transactions pour les 3 derniers mois (voir section "Données de Test" ci-dessous).

---

## ?? Données de Test Complètes

### Script de création rapide (via Swagger)

**Transactions Janvier 2025 :**
```json
# Revenu
POST /api/transactions
{"description": "Salaire", "amount": 3000, "type": "Income", "category": "Salaire", "date": "2025-01-15T00:00:00Z"}

# Dépenses
POST /api/transactions
{"description": "Courses", "amount": 450, "type": "Expense", "category": "Alimentation", "date": "2025-01-05T00:00:00Z"}

POST /api/transactions
{"description": "Loyer", "amount": 800, "type": "Expense", "category": "Logement", "date": "2025-01-01T00:00:00Z"}

POST /api/transactions
{"description": "Essence", "amount": 120, "type": "Expense", "category": "Transport", "date": "2025-01-10T00:00:00Z"}

POST /api/transactions
{"description": "Netflix", "amount": 15.99, "type": "Expense", "category": "Loisirs", "date": "2025-01-01T00:00:00Z"}
```

---

**Transactions Décembre 2024 :**
```json
POST /api/transactions
{"description": "Salaire", "amount": 3000, "type": "Income", "category": "Salaire", "date": "2024-12-15T00:00:00Z"}

POST /api/transactions
{"description": "Courses", "amount": 420, "type": "Expense", "category": "Alimentation", "date": "2024-12-10T00:00:00Z"}

POST /api/transactions
{"description": "Loyer", "amount": 800, "type": "Expense", "category": "Logement", "date": "2024-12-01T00:00:00Z"}
```

---

**Transactions Novembre 2024 :**
```json
POST /api/transactions
{"description": "Salaire", "amount": 3000, "type": "Income", "category": "Salaire", "date": "2024-11-15T00:00:00Z"}

POST /api/transactions
{"description": "Courses", "amount": 400, "type": "Expense", "category": "Alimentation", "date": "2024-11-08T00:00:00Z"}

POST /api/transactions
{"description": "Loyer", "amount": 800, "type": "Expense", "category": "Logement", "date": "2024-11-01T00:00:00Z"}
```

---

## ?? Checklist Complète de Tests

### Assets
- [ ] ? POST /api/assets (créer actif immobilier)
- [ ] ? POST /api/assets (créer véhicule)
- [ ] ? POST /api/assets (créer investissement)
- [ ] ? GET /api/assets (lister tous)
- [ ] ? GET /api/assets/{id} (récupérer par ID)
- [ ] ? GET /api/assets/total-value (valeur totale)
- [ ] ? PUT /api/assets/{id} (modifier)
- [ ] ? DELETE /api/assets/{id} (supprimer)

### Finance IA
- [ ] ? GET /api/finance/advice (conseil financier)
- [ ] ? POST /api/finance/suggest-category (suggestion catégorie)
- [ ] ? GET /api/finance/summary (résumé période)
- [ ] ? GET /api/finance/anomalies (détection anomalies)
- [ ] ? GET /api/finance/predict (prédiction budget)

### Transactions (vérification régression)
- [ ] ? GET /api/transactions (lister)
- [ ] ? POST /api/transactions (créer)
- [ ] ? GET /api/transactions/balance (solde)

---

## ?? Tests d'Erreurs

### Validation

**Test 1 : Créer un actif sans nom**
```json
POST /api/assets
{
  "name": "",
  "value": 100000,
  "type": "RealEstate",
  "acquisitionDate": "2020-01-01T00:00:00Z"
}
```
**Résultat attendu :** `400 Bad Request` (validation automatique)

---

**Test 2 : Récupérer un actif inexistant**
```
GET /api/assets/999
```
**Résultat attendu :** `404 Not Found`

---

**Test 3 : Suggestion catégorie sans description**
```json
POST /api/finance/suggest-category
{
  "description": "",
  "amount": 50
}
```
**Résultat attendu :** `400 Bad Request` + `"La description est requise"`

---

**Test 4 : Résumé avec dates invalides**
```
GET /api/finance/summary?startDate=2025-12-31&endDate=2025-01-01
```
**Résultat attendu :** `400 Bad Request` + `"La date de début doit être antérieure..."`

---

**Test 5 : Prédiction avec mois invalide**
```
GET /api/finance/predict?monthsAhead=15
```
**Résultat attendu :** `400 Bad Request` + `"Le nombre de mois doit être entre 1 et 12"`

---

## ?? Conseils de Test

### Ordre recommandé
1. **D'abord** : Créer des transactions (3 mois d'historique)
2. **Ensuite** : Tester les endpoints IA (summary, anomalies, predict)
3. **Enfin** : Créer des assets et tester le CRUD complet

### Utilisation efficace de Swagger
- ?? **Try it out** : Cliquez pour activer l'édition
- ?? **Execute** : Lancez la requête
- ?? **Response body** : Vérifiez le résultat
- ?? **Curl** : Copiez la commande curl si besoin

### Logs en temps réel
Regardez le terminal où l'API tourne pour voir les logs :
- `LogInformation` : Entrées/sorties normales
- `LogWarning` : Validations échouées
- `LogError` : Erreurs serveur

---

## ?? Validation Finale

**Si tous les tests passent :**
- ? Backend complet à 95%
- ? Prêt pour le frontend (Sprint 3)
- ? API production-ready

**En cas d'erreur :**
- Consultez `TROUBLESHOOTING.md`
- Vérifiez que PostgreSQL tourne : `docker ps`
- Vérifiez la clé API Gemini : `dotnet user-secrets list`

---

**Bon test ! ??**
