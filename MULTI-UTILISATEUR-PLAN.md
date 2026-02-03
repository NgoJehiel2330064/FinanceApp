# 🔐 Correction Multi-Utilisateur - Plan Complet

## 🎯 Objectif
Chaque utilisateur ne doit voir et modifier QUE ses propres données.

## 🔴 Problème Actuel
- ❌ Toutes les transactions sont retournées au endpoint GET /api/transactions
- ❌ Pas de filtrage par userId au backend
- ❌ Frontend n'envoie pas le userId aux requêtes

## ✅ Solution

### 1. Backend - Endpoint Corrections

#### TransactionsController
```csharp
// AVANT (Incorrect)
[HttpGet]
public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
{
    var transactions = await _context.Transactions
        .OrderByDescending(t => t.Date)
        .ToListAsync();
    return Ok(transactions);
}

// APRES (Correct - Multi-utilisateur)
[HttpGet]
public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions([FromQuery] int userId)
{
    if (userId <= 0)
        return BadRequest(new { message = "userId invalide" });

    var transactions = await _context.Transactions
        .Where(t => t.UserId == userId)  // ← FILTRAGE PAR UTILISATEUR
        .OrderByDescending(t => t.Date)
        .ToListAsync();
    
    return Ok(transactions);
}
```

**Autres méthodes à mettre à jour:**
- `GetTransaction(int id)` - Vérifier que la transaction appartient à l'utilisateur
- `CreateTransaction()` - Assigner userId automatiquement
- `UpdateTransaction()` - Vérifier l'ownership
- `DeleteTransaction()` - Vérifier l'ownership

#### AssetsController
Même pattern:
- `GetAssets([FromQuery] int userId)` - Filtrer par userId
- `GetAsset(int id, int userId)` - Vérifier ownership
- Etc.

#### FinanceController
- `GetAdvice([FromQuery] int userId)` - Contexte utilisateur
- `GetSummary([FromQuery] int userId)` - Données utilisateur
- Etc.

### 2. Frontend - API Calls Corrections

#### Récupérer userId depuis localStorage
```typescript
// Récupérer dans les pages
const userStr = localStorage.getItem('user');
const user = userStr ? JSON.parse(userStr) : null;
const userId = user?.id;
```

#### Mettre à jour toutes les requêtes API
```typescript
// AVANT (Universel)
const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS));

// APRES (Multi-utilisateur)
const response = await fetch(
  `${getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS)}?userId=${userId}`
);
```

**Pages à mettre à jour:**
- ✅ `app/page.tsx` - Dashboard (transactions, assets)
- ✅ `app/transactions/page.tsx` - Liste transactions
- ✅ `app/statistiques/page.tsx` - Stats utilisateur
- ✅ `app/patrimoine/page.tsx` - Assets utilisateur
- ✅ `app/profil/page.tsx` - Profile utilisateur

### 3. Pattern Service à Créer

```typescript
// lib/multi-user-service.ts
export function getUserId(): number | null {
  try {
    const userStr = localStorage.getItem('user');
    if (!userStr) return null;
    const user = JSON.parse(userStr);
    return user.id || null;
  } catch {
    return null;
  }
}

export function buildUserUrl(endpoint: string): string {
  const userId = getUserId();
  if (!userId) throw new Error('Utilisateur non authentifié');
  return `${getApiUrl(endpoint)}?userId=${userId}`;
}
```

## 📋 Checklist d'Implémentation

### Backend
- [ ] Corriger TransactionsController
  - [ ] GetTransactions([FromQuery] int userId)
  - [ ] GetTransaction(int id, [FromQuery] int userId)
  - [ ] CreateTransaction - Assigner userId
  - [ ] UpdateTransaction - Vérifier userId
  - [ ] DeleteTransaction - Vérifier userId
- [ ] Corriger AssetsController
  - [ ] GetAssets([FromQuery] int userId)
  - [ ] GetAsset(int id, [FromQuery] int userId)
  - [ ] CreateAsset - Assigner userId
  - [ ] UpdateAsset - Vérifier userId
  - [ ] DeleteAsset - Vérifier userId
- [ ] Corriger FinanceController
  - [ ] GetAdvice([FromQuery] int userId)
  - [ ] GetSummary([FromQuery] int userId)
  - [ ] GetPortfolioInsights([FromQuery] int userId)

### Frontend
- [ ] Créer lib/multi-user-service.ts
- [ ] Corriger app/page.tsx
- [ ] Corriger app/transactions/page.tsx
- [ ] Corriger app/statistiques/page.tsx
- [ ] Corriger app/patrimoine/page.tsx
- [ ] Corriger services (transaction-service.ts, etc)

### Test
- [ ] Créer 2 utilisateurs
- [ ] Vérifier Utilisateur 1 ne voit que ses données
- [ ] Vérifier Utilisateur 2 ne voit que ses données
- [ ] Modifier données Utilisateur 1 - Utilisateur 2 ne les voit pas

## ⚠️ Points de Sécurité

### Backend
```csharp
// ✅ BON - Vérifier l'userId
var transaction = await _context.Transactions
    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

if (transaction == null)
    return Forbidden("Accès refusé");
```

### Frontend
```typescript
// ✅ TOUJOURS inclure userId
fetch(`/api/transactions?userId=${userId}`);

// ❌ JAMAIS faire un appel sans userId
fetch(`/api/transactions`);  // ← DANGEREUX!
```

## 🚀 Ordre d'Exécution

1. Backend TransactionsController
2. Backend AssetsController
3. Frontend services
4. Frontend pages (page.tsx files)
5. Test complet

## 📊 Impact

| Aspect | Avant | Après |
|--------|-------|-------|
| Données Jean | Voit tout | Voit seulement ses données |
| Données Marie | Voit tout | Voit seulement ses données |
| Sécurité | 🔴 Critique | 🟢 Sécurisé |
| Multi-user | ❌ Non | ✅ Oui |

---

**Status**: 🔴 À FAIRE - Critique pour multi-utilisateur
