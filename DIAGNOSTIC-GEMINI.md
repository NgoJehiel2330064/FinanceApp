# 🔍 Diagnostic : Problème API Gemini

## ❌ Erreur reçue
```json
{"advice":"Impossible de générer un conseil pour le moment. Vérifiez votre configuration."}
```

## 🔧 Solutions possibles

### 1. ✅ Vérifier que l'API C# est lancée
```bash
cd FinanceApp
dotnet run
```
Attendez le message : `Now listening on: https://localhost:7219`

### 2. ✅ Vérifier qu'il y a des transactions en base
Le conseil IA nécessite au moins une transaction pour fonctionner.

**Option A : Via Postman/REST Client**
```http
POST https://localhost:7219/api/transactions
Content-Type: application/json

{
  "amount": 1500,
  "description": "Salaire",
  "category": "Salaire",
  "type": 1,
  "date": "2026-02-01T00:00:00Z"
}
```

**Option B : Via le frontend**
Utilisez le bouton "Ajouter une transaction" dans le dashboard.

### 3. ✅ Tester directement l'API Gemini

Ouvrez PowerShell et exécutez :

```powershell
$apiKey = "AIzaSyCpYUPvjgvhPNtCjlJDg0ddmwCXPvUZRCg"
$url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=$apiKey"
$body = @{
    contents = @(
        @{
            parts = @(
                @{ text = "Donne un conseil financier en 15 mots maximum." }
            )
        }
    )
    generationConfig = @{
        temperature = 0.3
        maxOutputTokens = 30
    }
} | ConvertTo-Json -Depth 10

$response = Invoke-RestMethod -Uri $url -Method Post -Body $body -ContentType "application/json"
$response.candidates[0].content.parts[0].text
```

**Si ça fonctionne** : Le problème vient du backend C#
**Si ça ne fonctionne pas** : Problème avec la clé API Gemini

### 4. ✅ Vérifier les logs du backend

Dans le terminal où tourne `dotnet run`, regardez les messages :

**✅ Messages positifs :**
```
info: FinanceApp.Services.GeminiService[0]
      Début de la génération du conseil financier
info: FinanceApp.Services.GeminiService[0]
      Récupération de X transactions
```

**❌ Messages d'erreur à chercher :**
```
warn: FinanceApp.Services.GeminiService[0]
      Clé API Gemini non configurée

fail: FinanceApp.Services.GeminiService[0]
      Erreur lors de la génération du conseil financier
```

### 5. ✅ Vérifier la configuration dans appsettings.json

Ouvrir : `FinanceApp/appsettings.json`

Vérifier que la section Gemini existe :
```json
"Gemini": {
  "ApiKey": "AIzaSyCpYUPvjgvhPNtCjlJDg0ddmwCXPvUZRCg",
  "Model": "gemini-1.5-flash",
  "Temperature": 0.3,
  "MaxTokens": 30
}
```

### 6. ✅ Vérifier que HttpClient est enregistré

Ouvrir : `FinanceApp/Program.cs`

Vérifier que ces lignes existent :
```csharp
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
builder.Services.AddScoped<IGeminiService, GeminiService>();
```

### 7. 🔄 Redémarrer complètement

```bash
# Arrêter tout (Ctrl+C dans chaque terminal)

# Terminal 1 : Redémarrer la base de données
docker-compose down
docker-compose up -d

# Terminal 2 : Redémarrer l'API C#
cd FinanceApp
dotnet clean
dotnet run

# Terminal 3 : Redémarrer le frontend
cd finance-ui
npm run dev
```

## 🎯 Checklist rapide

- [ ] L'API C# tourne sur https://localhost:7219
- [ ] Il y a au moins 1 transaction en base de données
- [ ] La clé API Gemini est dans appsettings.json
- [ ] Les logs backend ne montrent pas d'erreur
- [ ] Le test direct de l'API Gemini fonctionne
- [ ] Le frontend affiche bien les transactions

## 📞 Besoin d'aide ?

Si après ces vérifications le problème persiste, partagez :
1. Les logs complets du backend C#
2. La réponse du test PowerShell (étape 3)
3. Le nombre de transactions en base
