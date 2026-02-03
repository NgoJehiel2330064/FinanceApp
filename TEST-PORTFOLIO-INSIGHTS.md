# ?? Test Rapide - Portfolio Insights

## ?? Démarrage

### 1. Lancer l'API
```powershell
.\start-app.ps1
```

### 2. Ouvrir Swagger
```powershell
start http://localhost:5152/swagger
```

---

## ?? Scénario de Test Complet

### Étape 1 : Créer des Assets

**Dans Swagger, expandre "Assets" ? POST /api/assets**

**Asset 1 : Appartement**
```json
{
  "name": "Appartement Paris 15e",
  "value": 320000,
  "type": "RealEstate",
  "acquisitionDate": "2020-06-15T00:00:00Z"
}
```

**Asset 2 : Actions**
```json
{
  "name": "Portefeuille Actions",
  "value": 85000,
  "type": "Investment",
  "acquisitionDate": "2021-01-10T00:00:00Z"
}
```

**Asset 3 : Voiture**
```json
{
  "name": "Tesla Model 3",
  "value": 45000,
  "type": "Vehicle",
  "acquisitionDate": "2022-03-20T00:00:00Z"
}
```

---

### Étape 2 : Créer des Transactions

**Dans Swagger, expandre "Transactions" ? POST /api/transactions**

**Transaction 1 : Salaire**
```json
{
  "description": "Salaire mensuel",
  "amount": 3500,
  "type": "Income",
  "category": "Salaire",
  "date": "2025-01-15T00:00:00Z"
}
```

**Transaction 2 : Loyer**
```json
{
  "description": "Loyer",
  "amount": 1200,
  "type": "Expense",
  "category": "Logement",
  "date": "2025-01-01T00:00:00Z"
}
```

**Transaction 3 : Courses**
```json
{
  "description": "Courses supermarché",
  "amount": 600,
  "type": "Expense",
  "category": "Alimentation",
  "date": "2025-01-10T00:00:00Z"
}
```

**Transaction 4 : Essence**
```json
{
  "description": "Plein d'essence",
  "amount": 80,
  "type": "Expense",
  "category": "Transport",
  "date": "2025-01-05T00:00:00Z"
}
```

---

### Étape 3 : Tester Portfolio Insights

**Dans Swagger, expandre "Finance" ? GET /api/finance/portfolio-insights**

1. Click "Try it out"
2. Click "Execute"

**Résultat attendu :**
```json
{
  "insights": [
    "Votre patrimoine de 450K CAD est principalement immobilier (71%), ce qui limite la liquidité.",
    "Vos revenus mensuels de 3500 CAD ne représentent que 0.78% de votre patrimoine, un ratio faible.",
    "Avec 19% d'actifs productifs (investissements), envisagez de diversifier davantage votre portefeuille."
  ]
}
```

---

## ? Vérifications

### Check 1 : Assets Créés
**GET /api/assets**

Devrait retourner 3 actifs avec valeur totale ~450K

### Check 2 : Transactions Créées
**GET /api/transactions**

Devrait retourner 4 transactions

### Check 3 : Insights Générés
**GET /api/finance/portfolio-insights**

Devrait retourner 3 insights cohérents

---

## ?? Tests PowerShell

### Test Rapide Complet
```powershell
# 1. Créer des assets
$assets = @(
    @{ name = "Appartement"; value = 320000; type = "RealEstate"; acquisitionDate = "2020-06-15T00:00:00Z" },
    @{ name = "Actions"; value = 85000; type = "Investment"; acquisitionDate = "2021-01-10T00:00:00Z" },
    @{ name = "Voiture"; value = 45000; type = "Vehicle"; acquisitionDate = "2022-03-20T00:00:00Z" }
)

foreach ($asset in $assets) {
    $json = $asset | ConvertTo-Json
    Invoke-RestMethod -Uri "http://localhost:5152/api/assets" `
                      -Method Post `
                      -Body $json `
                      -ContentType "application/json"
}

# 2. Créer des transactions
$transactions = @(
    @{ description = "Salaire"; amount = 3500; type = "Income"; category = "Salaire"; date = "2025-01-15T00:00:00Z" },
    @{ description = "Loyer"; amount = 1200; type = "Expense"; category = "Logement"; date = "2025-01-01T00:00:00Z" },
    @{ description = "Courses"; amount = 600; type = "Expense"; category = "Alimentation"; date = "2025-01-10T00:00:00Z" }
)

foreach ($transaction in $transactions) {
    $json = $transaction | ConvertTo-Json
    Invoke-RestMethod -Uri "http://localhost:5152/api/transactions" `
                      -Method Post `
                      -Body $json `
                      -ContentType "application/json"
}

# 3. Tester portfolio insights
$insights = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/portfolio-insights"

Write-Host "`n?? INSIGHTS PATRIMONIAUX :" -ForegroundColor Cyan
$insights.insights | ForEach-Object { Write-Host "   • $_" -ForegroundColor Yellow }
```

---

## ?? Interprétation des Résultats

### Insight 1 : Structure du Patrimoine
**Exemple :**
```
"Votre patrimoine de 450K CAD est principalement immobilier (71%), ce qui limite la liquidité."
```

**Interprétation :**
- ? Valeur totale calculée correctement
- ? Répartition par type détectée
- ? Point d'attention sur la concentration

---

### Insight 2 : Ratio Revenus / Patrimoine
**Exemple :**
```
"Vos revenus mensuels de 3500 CAD ne représentent que 0.78% de votre patrimoine, un ratio faible."
```

**Interprétation :**
- ? Revenus mensuels moyens calculés
- ? Ratio calculé et comparé
- ? Évaluation qualitative (faible/normal/élevé)

---

### Insight 3 : Assets Productifs
**Exemple :**
```
"Avec 19% d'actifs productifs (investissements), envisagez de diversifier davantage votre portefeuille."
```

**Interprétation :**
- ? Heuristique Investment = productif
- ? Pourcentage calculé
- ? Recommandation contextuelle

---

## ?? Tests d'Erreurs

### Test 1 : Sans Assets
**Supprimer tous les assets :**
```powershell
$assets = Invoke-RestMethod -Uri "http://localhost:5152/api/assets"
foreach ($asset in $assets) {
    Invoke-RestMethod -Uri "http://localhost:5152/api/assets/$($asset.id)" -Method Delete
}
```

**Tester insights :**
```powershell
$result = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/portfolio-insights"
$result.insights
```

**Résultat attendu :**
```
Aucun asset enregistré. Commencez par ajouter vos biens pour une analyse patrimoniale complète.
```

---

### Test 2 : Sans Transactions (mais avec Assets)
**Supprimer toutes les transactions :**
```powershell
$transactions = Invoke-RestMethod -Uri "http://localhost:5152/api/transactions"
foreach ($transaction in $transactions) {
    Invoke-RestMethod -Uri "http://localhost:5152/api/transactions/$($transaction.id)" -Method Delete
}
```

**Tester insights :**
```powershell
$result = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/portfolio-insights"
$result.insights
```

**Résultat attendu :**
- Insights basés uniquement sur les assets
- Revenus = 0
- Ratio revenus/patrimoine = 0%

---

## ?? Critères de Succès

### ? Test Réussi Si :
1. **3 insights retournés** (ou 1 si pas d'assets)
2. **Chiffres cohérents** avec les assets/transactions créés
3. **Ton professionnel** et phrases complètes
4. **Pas d'erreurs 500** ni de timeouts
5. **Temps de réponse < 5s**

### ? Test Échoué Si :
- Insights vides ou "null"
- Chiffres incohérents
- Erreur 500
- Insights génériques sans chiffres

---

## ?? Scénarios Avancés

### Scénario 1 : Patrimoine Immobilier Dominant
**Créer :**
- 3 biens immobiliers (total 800K)
- 1 véhicule (30K)
- Transactions faibles

**Insight attendu :**
```
"Forte concentration immobilière (96%), envisagez de diversifier vers des actifs liquides."
```

---

### Scénario 2 : Portfolio Diversifié
**Créer :**
- 1 bien immobilier (200K)
- 3 investissements (180K)
- 1 véhicule (20K)

**Insight attendu :**
```
"Excellente diversification avec 45% en investissements productifs."
```

---

### Scénario 3 : Jeune Professionnel
**Créer :**
- 1 investissement (20K)
- Transactions avec revenus élevés (5K/mois)

**Insight attendu :**
```
"Ratio revenus élevé (25%/mois) favorise une accumulation rapide du capital."
```

---

## ?? Validation Finale

**Si tous les tests passent :**
- ? Portfolio Insights fonctionnel
- ? Intégration IA opérationnelle
- ? Calculs patrimoniaux corrects
- ? Backend complet à 97%

---

**Bon test ! ??**
