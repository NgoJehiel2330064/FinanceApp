# ?? Exemples d'Utilisation - FinanceApp API

Ce document contient des exemples concrets d'utilisation de l'API FinanceApp avec PowerShell, curl et JavaScript.

---

## ?? Table des Matières

1. [Assets (Patrimoine)](#assets-patrimoine)
2. [Finance IA](#finance-ia)
3. [Transactions (rappel)](#transactions-rappel)
4. [Scénarios Complets](#scénarios-complets)

---

## ?? Assets (Patrimoine)

### 1. Créer un bien immobilier

**PowerShell :**
```powershell
$asset = @{
    name = "Appartement Paris 15e"
    value = 320000
    type = "RealEstate"
    acquisitionDate = "2020-06-15T00:00:00Z"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5152/api/assets" `
                  -Method Post `
                  -Body $asset `
                  -ContentType "application/json"
```

**curl (Git Bash) :**
```bash
curl -X POST http://localhost:5152/api/assets \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Appartement Paris 15e",
    "value": 320000,
    "type": "RealEstate",
    "acquisitionDate": "2020-06-15T00:00:00Z"
  }'
```

**JavaScript (fetch) :**
```javascript
const response = await fetch('http://localhost:5152/api/assets', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    name: 'Appartement Paris 15e',
    value: 320000,
    type: 'RealEstate',
    acquisitionDate: '2020-06-15T00:00:00Z'
  })
});

const asset = await response.json();
console.log('Asset créé:', asset);
```

---

### 2. Lister tous les actifs

**PowerShell :**
```powershell
$assets = Invoke-RestMethod -Uri "http://localhost:5152/api/assets"
$assets | Format-Table Id, Name, Value, Type
```

**curl :**
```bash
curl http://localhost:5152/api/assets | jq
```

**JavaScript :**
```javascript
const assets = await fetch('http://localhost:5152/api/assets')
  .then(res => res.json());

assets.forEach(asset => {
  console.log(`${asset.name}: ${asset.value}€`);
});
```

---

### 3. Obtenir la valeur totale du patrimoine

**PowerShell :**
```powershell
$totalValue = Invoke-RestMethod -Uri "http://localhost:5152/api/assets/total-value"
Write-Host "Valeur totale du patrimoine : $totalValue€" -ForegroundColor Green
```

**curl :**
```bash
curl http://localhost:5152/api/assets/total-value
```

**JavaScript :**
```javascript
const totalValue = await fetch('http://localhost:5152/api/assets/total-value')
  .then(res => res.json());

console.log(`Patrimoine total: ${totalValue.toLocaleString('fr-FR')}€`);
```

---

### 4. Modifier un actif

**PowerShell :**
```powershell
$updatedAsset = @{
    id = 1
    name = "Appartement Paris 15e (rénové)"
    value = 350000
    type = "RealEstate"
    acquisitionDate = "2020-06-15T00:00:00Z"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5152/api/assets/1" `
                  -Method Put `
                  -Body $updatedAsset `
                  -ContentType "application/json"
```

**curl :**
```bash
curl -X PUT http://localhost:5152/api/assets/1 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "Appartement Paris 15e (rénové)",
    "value": 350000,
    "type": "RealEstate",
    "acquisitionDate": "2020-06-15T00:00:00Z"
  }'
```

---

### 5. Supprimer un actif

**PowerShell :**
```powershell
Invoke-RestMethod -Uri "http://localhost:5152/api/assets/3" -Method Delete
```

**curl :**
```bash
curl -X DELETE http://localhost:5152/api/assets/3
```

**JavaScript :**
```javascript
await fetch('http://localhost:5152/api/assets/3', {
  method: 'DELETE'
});
```

---

## ?? Finance IA

### 1. Obtenir un conseil financier

**PowerShell :**
```powershell
$advice = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/advice"
Write-Host "?? Conseil : $($advice.advice)" -ForegroundColor Cyan
```

**curl :**
```bash
curl http://localhost:5152/api/finance/advice
```

**JavaScript :**
```javascript
const { advice } = await fetch('http://localhost:5152/api/finance/advice')
  .then(res => res.json());

console.log('?? Conseil:', advice);
```

---

### 2. Suggérer une catégorie

**PowerShell :**
```powershell
$request = @{
    description = "Courses Lidl"
    amount = 85.50
} | ConvertTo-Json

$result = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/suggest-category" `
                             -Method Post `
                             -Body $request `
                             -ContentType "application/json"

Write-Host "Catégorie suggérée : $($result.category)" -ForegroundColor Yellow
```

**curl :**
```bash
curl -X POST http://localhost:5152/api/finance/suggest-category \
  -H "Content-Type: application/json" \
  -d '{"description": "Courses Lidl", "amount": 85.50}'
```

**JavaScript :**
```javascript
const { category } = await fetch('http://localhost:5152/api/finance/suggest-category', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    description: 'Courses Lidl',
    amount: 85.50
  })
}).then(res => res.json());

console.log('Catégorie:', category);
```

---

### 3. Résumé financier

**PowerShell :**
```powershell
$startDate = "2025-01-01"
$endDate = "2025-01-31"

$summary = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/summary?startDate=$startDate&endDate=$endDate"
Write-Host "?? Résumé : $($summary.summary)" -ForegroundColor Green
```

**curl :**
```bash
curl "http://localhost:5152/api/finance/summary?startDate=2025-01-01&endDate=2025-01-31"
```

**JavaScript :**
```javascript
const startDate = '2025-01-01';
const endDate = '2025-01-31';

const { summary } = await fetch(
  `http://localhost:5152/api/finance/summary?startDate=${startDate}&endDate=${endDate}`
).then(res => res.json());

console.log('?? Résumé:', summary);
```

---

### 4. Détecter les anomalies

**PowerShell :**
```powershell
$result = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/anomalies"
Write-Host "?? Anomalies détectées :" -ForegroundColor Red
$result.anomalies | ForEach-Object { Write-Host "   • $_" }
```

**curl :**
```bash
curl http://localhost:5152/api/finance/anomalies | jq '.anomalies[]'
```

**JavaScript :**
```javascript
const { anomalies } = await fetch('http://localhost:5152/api/finance/anomalies')
  .then(res => res.json());

console.log('?? Anomalies:');
anomalies.forEach(anomaly => console.log(`   • ${anomaly}`));
```

---

### 5. Prédiction budgétaire

**PowerShell :**
```powershell
$monthsAhead = 6
$result = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/predict?monthsAhead=$monthsAhead"
Write-Host "?? Prédiction : $($result.prediction)" -ForegroundColor Magenta
```

**curl :**
```bash
curl "http://localhost:5152/api/finance/predict?monthsAhead=6"
```

**JavaScript :**
```javascript
const { prediction } = await fetch('http://localhost:5152/api/finance/predict?monthsAhead=6')
  .then(res => res.json());

console.log('?? Prédiction:', prediction);
```

---

## ?? Transactions (rappel)

### Créer une transaction

**PowerShell :**
```powershell
$transaction = @{
    description = "Salaire mensuel"
    amount = 3000
    type = "Income"
    category = "Salaire"
    date = (Get-Date).ToString("o")
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5152/api/transactions" `
                  -Method Post `
                  -Body $transaction `
                  -ContentType "application/json"
```

### Obtenir le solde

**PowerShell :**
```powershell
$balance = Invoke-RestMethod -Uri "http://localhost:5152/api/transactions/balance"
Write-Host "Solde actuel : $balance€" -ForegroundColor $(if($balance -gt 0){'Green'}else{'Red'})
```

---

## ?? Scénarios Complets

### Scénario 1 : Nouveau bien immobilier

```powershell
# 1. Créer l'actif
$appartement = @{
    name = "Studio Lyon"
    value = 150000
    type = "RealEstate"
    acquisitionDate = "2024-03-01T00:00:00Z"
} | ConvertTo-Json

$created = Invoke-RestMethod -Uri "http://localhost:5152/api/assets" `
                              -Method Post `
                              -Body $appartement `
                              -ContentType "application/json"

Write-Host "? Actif créé avec ID $($created.id)" -ForegroundColor Green

# 2. Vérifier la valeur totale
$total = Invoke-RestMethod -Uri "http://localhost:5152/api/assets/total-value"
Write-Host "?? Valeur totale du patrimoine : $total€" -ForegroundColor Cyan
```

---

### Scénario 2 : Analyse financière mensuelle

```powershell
# 1. Obtenir le résumé du mois
$summary = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/summary?startDate=2025-01-01&endDate=2025-01-31"
Write-Host "?? $($summary.summary)" -ForegroundColor Cyan

# 2. Détecter les anomalies
$anomalies = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/anomalies"
if ($anomalies.anomalies.Count -gt 0) {
    Write-Host "`n?? Anomalies détectées :" -ForegroundColor Yellow
    $anomalies.anomalies | ForEach-Object { Write-Host "   • $_" }
}

# 3. Conseil personnalisé
$advice = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/advice"
Write-Host "`n?? $($advice.advice)" -ForegroundColor Green
```

---

### Scénario 3 : Ajout rapide de transaction avec catégorie IA

```powershell
# 1. Suggérer une catégorie
$description = "Restaurant La Belle Époque"
$amount = 65.00

$categoryRequest = @{
    description = $description
    amount = $amount
} | ConvertTo-Json

$suggested = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/suggest-category" `
                                -Method Post `
                                -Body $categoryRequest `
                                -ContentType "application/json"

Write-Host "?? Catégorie suggérée : $($suggested.category)" -ForegroundColor Cyan

# 2. Créer la transaction avec la catégorie suggérée
$transaction = @{
    description = $description
    amount = $amount
    type = "Expense"
    category = $suggested.category
    date = (Get-Date).ToString("o")
} | ConvertTo-Json

$created = Invoke-RestMethod -Uri "http://localhost:5152/api/transactions" `
                              -Method Post `
                              -Body $transaction `
                              -ContentType "application/json"

Write-Host "? Transaction créée : $($created.description) - $($created.amount)€ ($($created.category))" -ForegroundColor Green
```

---

### Scénario 4 : Dashboard complet

```powershell
Write-Host "???????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "      DASHBOARD FINANCIER - FinanceApp      " -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# 1. Transactions
$balance = Invoke-RestMethod -Uri "http://localhost:5152/api/transactions/balance"
$balanceColor = if($balance -gt 0){'Green'}else{'Red'}
Write-Host "?? Solde : " -NoNewline
Write-Host "$balance€" -ForegroundColor $balanceColor

# 2. Assets
$totalAssets = Invoke-RestMethod -Uri "http://localhost:5152/api/assets/total-value"
Write-Host "?? Patrimoine : $totalAssets€" -ForegroundColor Green

# 3. Valeur nette
$netWorth = $balance + $totalAssets
Write-Host "?? Valeur nette : $netWorth€" -ForegroundColor Cyan

# 4. Conseil IA
Write-Host ""
$advice = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/advice"
Write-Host "?? Conseil IA : " -NoNewline
Write-Host "$($advice.advice)" -ForegroundColor Yellow

# 5. Prédiction
$prediction = Invoke-RestMethod -Uri "http://localhost:5152/api/finance/predict?monthsAhead=3"
Write-Host "?? Prédiction 3 mois : " -NoNewline
Write-Host "$($prediction.prediction)" -ForegroundColor Magenta

Write-Host ""
Write-Host "???????????????????????????????????????????" -ForegroundColor Cyan
```

---

## ?? Configuration Frontend (Next.js/React)

### Configuration API

```typescript
// config/api.ts
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5152';

export const API_ENDPOINTS = {
  // Transactions
  transactions: `${API_BASE_URL}/api/transactions`,
  transactionsBalance: `${API_BASE_URL}/api/transactions/balance`,
  
  // Assets
  assets: `${API_BASE_URL}/api/assets`,
  assetsTotalValue: `${API_BASE_URL}/api/assets/total-value`,
  
  // Finance IA
  financeAdvice: `${API_BASE_URL}/api/finance/advice`,
  financeSuggestCategory: `${API_BASE_URL}/api/finance/suggest-category`,
  financeSummary: `${API_BASE_URL}/api/finance/summary`,
  financeAnomalies: `${API_BASE_URL}/api/finance/anomalies`,
  financePredict: `${API_BASE_URL}/api/finance/predict`,
};
```

### Hooks personnalisés

```typescript
// hooks/useFinanceAdvice.ts
import { useState, useEffect } from 'react';
import { API_ENDPOINTS } from '@/config/api';

export function useFinanceAdvice() {
  const [advice, setAdvice] = useState<string>('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch(API_ENDPOINTS.financeAdvice)
      .then(res => res.json())
      .then(data => setAdvice(data.advice))
      .finally(() => setLoading(false));
  }, []);

  return { advice, loading };
}
```

```typescript
// hooks/useAssets.ts
import useSWR from 'swr';
import { API_ENDPOINTS } from '@/config/api';

const fetcher = (url: string) => fetch(url).then(res => res.json());

export function useAssets() {
  const { data, error, mutate } = useSWR(API_ENDPOINTS.assets, fetcher);

  return {
    assets: data || [],
    isLoading: !error && !data,
    isError: error,
    refresh: mutate,
  };
}
```

---

## ?? Exemple Composant React

```tsx
// components/FinanceDashboard.tsx
import { useFinanceAdvice } from '@/hooks/useFinanceAdvice';
import { useAssets } from '@/hooks/useAssets';

export default function FinanceDashboard() {
  const { advice, loading: adviceLoading } = useFinanceAdvice();
  const { assets, isLoading: assetsLoading } = useAssets();

  if (adviceLoading || assetsLoading) return <div>Chargement...</div>;

  const totalValue = assets.reduce((sum, asset) => sum + asset.value, 0);

  return (
    <div className="dashboard">
      <h1>Dashboard Financier</h1>
      
      <div className="card">
        <h2>?? Conseil IA</h2>
        <p>{advice}</p>
      </div>

      <div className="card">
        <h2>?? Patrimoine</h2>
        <p className="amount">{totalValue.toLocaleString('fr-FR')}€</p>
        <ul>
          {assets.map(asset => (
            <li key={asset.id}>
              {asset.name}: {asset.value.toLocaleString('fr-FR')}€
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}
```

---

## ?? Vous êtes prêt !

Tous les exemples ci-dessus sont fonctionnels et testés.

**Référez-vous à :**
- `SWAGGER-TESTS-GUIDE.md` pour les tests Swagger
- `FRONTEND-CONFIGURATION.md` pour la configuration complète du frontend
- `CAHIER-DES-CHARGES.md` pour la documentation complète

**Bon développement ! ??**
