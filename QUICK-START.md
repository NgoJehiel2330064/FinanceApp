# 🚀 QUICK START - IA Analytics v2.0

**Last Updated**: 2026-02-02  
**Status**: ✅ Production Ready

---

## ⚡ 30-Second Setup

```bash
# Terminal 1: Start Backend
cd "c:\Users\GOAT\OneDrive\Documents\FinanceApp\FinanceApp"
dotnet run
# Opens: http://localhost:5153

# Terminal 2: Start Frontend
cd "c:\Users\GOAT\OneDrive\Documents\FinanceApp\finance-ui"
npm run dev
# Opens: http://localhost:3000
```

---

## 🎯 What's New?

### 3 Powerful AI Endpoints

| Endpoint | Purpose | Example |
|----------|---------|---------|
| `/api/finance/spending-patterns` | Analyze spending habits | Total spent, variance, trends |
| `/api/finance/smart-anomalies` | Detect unusual expenses | 450 CAD when avg is 50 CAD |
| `/api/finance/recommendations` | Get optimization tips | Reduce Alimentation by 10% → +128/mo |

### New Frontend Page

**URL**: http://localhost:3000/ia-analytics

**Features**:
- 📊 Spending Patterns (with categories breakdown)
- ⚠️ Anomalies (High/Medium/Low severity)
- 💡 Recommendations (with potential savings)

---

## 🔐 Authentication

```
1. Go to: http://localhost:3000/connexion
2. Login with email/password
3. Token stored automatically in sessionStorage
4. All API calls include Authorization header
5. Logout clears token (automatic)
```

---

## 📊 Sample Data

### View Spending Patterns
```json
{
  "totalTransactions": 156,
  "totalSpent": 4280.50,
  "averageMonthlySpending": 1426.83,
  "spendingVariance": 15.3,
  "trendDirection": "Decreasing",
  "categories": [...]
}
```

### Check Anomalies
```json
{
  "totalAnomalies": 5,
  "highSeverityCount": 2,
  "anomalies": [
    {
      "description": "Restaurant Premium",
      "amount": 450.00,
      "severity": "High",
      "message": "355% above average"
    }
  ]
}
```

### Get Recommendations
```json
{
  "recommendations": [
    {
      "title": "Reduce Alimentation spending",
      "description": "Category is 30% of budget",
      "potentialSavings": 128.54,
      "priority": "High"
    }
  ]
}
```

---

## 🧪 Quick Test

### Test in Browser

1. Open: http://localhost:3000/ia-analytics
2. Should see 3 tabs: Patterns | Anomalies | Recommendations
3. Click each tab to view different analyses
4. Scroll to see all recommendations

### Test API with curl

```bash
# Get JWT token first
curl -X POST http://localhost:5153/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@test.com","password":"password"}'

# Copy the token, then:
curl http://localhost:5153/api/finance/spending-patterns?userId=1 \
  -H "Authorization: Bearer {token}"
```

---

## 🛠️ Common Commands

### Backend
```bash
# Build only
dotnet build

# Build and run
dotnet run

# Run tests
dotnet test

# Check migrations
dotnet ef migrations list
```

### Frontend
```bash
# Build only
npm run build

# Run development
npm run dev

# Run tests
npm run test

# Check for errors
npm run lint
```

---

## ⚙️ Configuration

### JWT Secret
Located in `appsettings.json` and `appsettings.Development.json`:
```json
"Jwt": {
  "Key": "Y0uR_sUp3r_s3cr3t_jwt_k3y_2025_F1N@nc3@pp!",
  "Issuer": "FinanceApp",
  "Audience": "FinanceAppUsers",
  "ExpiresMinutes": 60
}
```

### API Endpoints
```
Base URL: http://localhost:5153/api
Auth: POST /auth/login
Finance: GET /finance/spending-patterns
         GET /finance/smart-anomalies
         GET /finance/recommendations
```

### Frontend URLs
```
Base URL: http://localhost:3000
Home: /
IA Analytics: /ia-analytics ← NEW!
Transactions: /transactions
Statistics: /statistiques
Assets: /patrimoine
Profile: /profil
```

---

## 🐛 Troubleshooting

### Backend won't start
```bash
# Check if port 5153 is in use
netstat -ano | findstr :5153

# Kill process
taskkill /PID {PID} /F

# Try again
dotnet run
```

### Frontend won't connect to API
```
Check:
1. Backend is running (http://localhost:5153)
2. CORS is enabled in Program.cs
3. JWT token is in sessionStorage (DevTools)
4. Authorization header is sent (Network tab)
```

### No anomalies detected
```
Reason: Need transactions with large variance
Solution: Add more transactions manually
         Or use test data script
```

---

## 📚 Documentation

| File | Purpose |
|------|---------|
| [IA-ANALYTICS-V2.0.md](IA-ANALYTICS-V2.0.md) | Complete feature docs |
| [SUMMARY-SESSION-IA-JWT.md](SUMMARY-SESSION-IA-JWT.md) | Session report |
| [VALIDATION-FINAL.md](VALIDATION-FINAL.md) | Testing & validation |
| [TEST-IA-ANALYTICS.http](TEST-IA-ANALYTICS.http) | API test scenarios |

---

## 🔒 Security Checklist

- [x] JWT token required for all IA endpoints
- [x] sessionStorage (not localStorage)
- [x] Ownership verification (userId check)
- [x] 403 Forbidden on unauthorized access
- [x] Token expires after 60 minutes
- [x] No userId in query strings

---

## 🎯 Key Features

### Spending Patterns 📊
- Total spent (3 months)
- Average monthly spending
- Variance (stability)
- Trend (increasing/decreasing)
- Top categories breakdown

### Anomaly Detection ⚠️
- High: > average + 2×stddev (>100% deviation)
- Medium: > average + stddev (50-100% deviation)
- Low: Rare categories
- With expected range

### Recommendations 💡
- ReduceSpending: Categories > 40%
- ReviewAnomalies: Critical transactions
- OptimizeRecurring: Repeating expenses
- DailyBudget: Recommended daily limit
- StabilizeSpending: High variance alerts

---

## 📞 Support

### If Something Breaks

1. **Check logs**:
   - Backend: Console output
   - Frontend: Browser DevTools Console
   - Network: Network tab (HTTP requests)

2. **Verify setup**:
   - PostgreSQL running?
   - Ports 5153 & 3000 available?
   - JWT key in appsettings?

3. **Reset state**:
   - Clear sessionStorage: DevTools > Application > Storage
   - Restart backend & frontend
   - Try again

### Need Help?
- Check `IA-ANALYTICS-V2.0.md` for detailed docs
- Review test scenarios in `TEST-IA-ANALYTICS.http`
- Check validation report in `VALIDATION-FINAL.md`

---

## ✨ Pro Tips

1. **Add test data quickly**:
   ```bash
   # Use Postman or Thunder Client
   POST http://localhost:5153/api/transactions
   { "description": "...", "amount": 50, "category": "Alimentation", "type": 0 }
   ```

2. **Monitor requests**:
   - DevTools > Network tab
   - Filter by `/finance`
   - Check response JSON

3. **Debug tokens**:
   - DevTools > Application > Cookies
   - Look for `auth_token`
   - Paste at jwt.io to decode

4. **Bulk test data**:
   ```
   Create 50-100 transactions with varied amounts
   → Will trigger anomaly detection
   → Recommendations will appear
   ```

---

## 🎉 You're All Set!

### Next Steps
1. ✅ Start backend
2. ✅ Start frontend  
3. ✅ Login to account
4. ✅ Go to /ia-analytics
5. ✅ See your personalized analysis!

---

**Version**: 2.0.0  
**Status**: ✅ **Ready to Use**  
**Last Check**: 2026-02-02  

Happy analyzing! 🚀
    type = "Income"
    category = "Salaire"
    date = (Get-Date).ToString("o")
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5152/api/transactions" `
                  -Method Post `
                  -Body $transaction `
                  -ContentType "application/json"

# GET - Conseil financier IA (Gemini)
Invoke-RestMethod -Uri "http://localhost:5152/api/finance/advice" -Method Get
```

---

### 3. Avec curl (Git Bash / Linux)

```bash
# GET - Liste des transactions
curl http://localhost:5152/api/transactions

# POST - Cr�er une transaction
curl -X POST http://localhost:5152/api/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Salaire mensuel",
    "amount": 3000,
    "type": "Income",
    "category": "Salaire",
    "date": "2025-02-01T00:00:00Z"
  }'

# GET - Conseil financier IA
curl http://localhost:5152/api/finance/advice
```

---

### 4. Avec votre navigateur

Ouvrez simplement ces URLs :
- http://localhost:5152/api/transactions
- http://localhost:5152/api/finance/advice

---

## ?? Endpoints disponibles

### Transactions

| M�thode | Endpoint | Description |
|---------|----------|-------------|
| `GET` | `/api/transactions` | Liste toutes les transactions |
| `GET` | `/api/transactions/{id}` | D�tails d'une transaction |
| `POST` | `/api/transactions` | Cr�er une transaction |
| `PUT` | `/api/transactions/{id}` | Modifier une transaction |
| `DELETE` | `/api/transactions/{id}` | Supprimer une transaction |
| `GET` | `/api/transactions/balance` | Calculer le solde total |

### Finance (IA Gemini)

| M�thode | Endpoint | Description |
|---------|----------|-------------|
| `GET` | `/api/finance/advice` | Conseil financier personnalis� |

---

## ?? Connecter votre Frontend

Si vous avez un frontend Next.js, React, Vue, etc., configurez l'URL de l'API :

```typescript
// config/api.ts
const API_BASE_URL = "http://localhost:5152";

export const API_CONFIG = {
  BASE_URL: API_BASE_URL,
  ENDPOINTS: {
    TRANSACTIONS: "/api/transactions",
    FINANCIAL_ADVICE: "/api/finance/advice",
  }
};
```

**Consultez `FRONTEND-CONFIGURATION.md`** pour des exemples complets.

---

## ?? Arr�ter l'application

### Avec le script

```powershell
.\stop-app.ps1
```

### Manuellement

```powershell
# 1. Dans le terminal de l'API : Ctrl+C

# 2. Arr�ter PostgreSQL (optionnel)
docker-compose down
```

---

## ? Probl�mes ?

### Erreur : "Port 5152 already in use"

```powershell
# Trouver le processus
netstat -ano | findstr :5152

# Tuer le processus (remplacez PID par le num�ro trouv�)
taskkill /F /PID <PID>
```

### Erreur : "Cannot connect to PostgreSQL"

```powershell
# V�rifier que PostgreSQL tourne
docker ps

# Si pas d�marr�
docker-compose up -d

# V�rifier les logs
docker logs postgres_db
```

### Erreur : "Cl� API Gemini non configur�e"

```powershell
cd FinanceApp
dotnet user-secrets set "Gemini:ApiKey" "AIzaSyDfU2oIqH7WQ825btkeddIWONEPnApF8Gs"
```

### Erreur frontend : "Failed to fetch"

1. ? V�rifiez que l'API est d�marr�e : http://localhost:5152/swagger
2. ? V�rifiez l'URL dans votre frontend : doit �tre `http://localhost:5152`
3. ? Consultez **TROUBLESHOOTING.md** pour les probl�mes CORS

---

## ?? Documentation compl�te

- **[README.md](README.md)** - Vue d'ensemble du projet
- **[SECRETS-CONFIGURATION.md](SECRETS-CONFIGURATION.md)** - Gestion des cl�s API
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** - R�solution de probl�mes d�taill�e
- **[FRONTEND-CONFIGURATION.md](FRONTEND-CONFIGURATION.md)** - Configuration du frontend
- **[test-config.ps1](test-config.ps1)** - Script de v�rification de la configuration

---

## ? Pr�t � coder !

Votre environnement est **100% configur�** ! ??

```powershell
# Lancez l'application
.\start-app.ps1

# Ouvrez Swagger
start http://localhost:5152/swagger

# Commencez � d�velopper votre frontend
# (l'API est pr�te � recevoir vos requ�tes !)
```

**Bon d�veloppement ! ??**
