# Guide de déploiement (résumé)

## 1) Backend sur Railway

1. Créez un projet Railway et importez le repo GitHub
2. Ajoutez PostgreSQL
3. Ajoutez les variables d'environnement :

```
ConnectionStrings__DefaultConnection = ${Postgres.DATABASE_URL}
Jwt__Key = <CHANGE_ME>
Jwt__Issuer = FinanceApp
Jwt__Audience = FinanceAppUsers
Jwt__ExpiresMinutes = 60
ASPNETCORE_ENVIRONMENT = Production
Groq__ApiKey = <CHANGE_ME>
Groq__Model = mixtral-8x7b-32768
Groq__BaseUrl = https://api.groq.com/openai/v1
Groq__Temperature = 0.3
Groq__MaxTokens = 150
```

4. Déployez et notez l'URL Railway
5. Vérifiez : `https://VOTRE-URL-RAILWAY/health`

## 2) Frontend sur Vercel

1. Importez le repo
2. Root Directory : `finance-ui`
3. Ajoutez la variable :

```
NEXT_PUBLIC_API_URL = https://VOTRE-URL-RAILWAY
```

4. Déployez et notez l'URL Vercel

## 3) CORS

Ajoutez sur Railway :

```
AllowedOrigins__0 = https://VOTRE-URL-VERCEL
```

## 4) Partage

Partagez l'URL Vercel : utilisable sur mobile, ordinateur et tablette.
