# Configuration des Secrets - FinanceApp

## ?? IMPORTANT : Sécurité de la clé API

**NE JAMAIS** committer les clés API dans Git ! Elles sont maintenant stockées de manière sécurisée.

---

## ?? Configuration en Développement (Local)

### User Secrets (Recommandé)

Les User Secrets sont stockés **en dehors du projet** et ne sont jamais commités.

#### 1?? Initialiser User Secrets (Déjà fait ?)

```bash
cd FinanceApp
dotnet user-secrets init
```

#### 2?? Configurer votre clé API Gemini (Déjà fait ?)

```bash
dotnet user-secrets set "Gemini:ApiKey" "VOTRE_CLE_API_GEMINI"
```

#### 3?? Vérifier les secrets configurés

```bash
dotnet user-secrets list
```

#### 4?? Localisation des secrets

Les secrets sont stockés dans :
- **Windows** : `%APPDATA%\Microsoft\UserSecrets\[UserSecretsId]\secrets.json`
- **macOS/Linux** : `~/.microsoft/usersecrets/[UserSecretsId]/secrets.json`

---

## ?? Configuration en Production

### Azure App Service

1. Allez dans **Configuration** ? **Application Settings**
2. Ajoutez une nouvelle setting :
   - **Nom** : `Gemini__ApiKey` (notez le double underscore)
   - **Valeur** : Votre clé API Gemini

### Docker

```bash
docker run -e Gemini__ApiKey="VOTRE_CLE_API" votre-image
```

Ou dans `docker-compose.yml` :

```yaml
services:
  app:
    environment:
      - Gemini__ApiKey=VOTRE_CLE_API
```

### Linux (SystemD, Bash)

```bash
export Gemini__ApiKey="VOTRE_CLE_API"
dotnet FinanceApp.dll
```

### Windows (PowerShell)

```powershell
$env:Gemini__ApiKey="VOTRE_CLE_API"
dotnet FinanceApp.dll
```

---

## ?? Obtenir une clé API Gemini

1. Allez sur [Google AI Studio](https://makersuite.google.com/app/apikey)
2. Connectez-vous avec votre compte Google
3. Cliquez sur **"Create API Key"**
4. Copiez la clé générée
5. Configurez-la avec User Secrets (voir ci-dessus)

---

## ?? Tester la configuration

Lancez l'application et appelez l'endpoint :

```bash
GET http://localhost:5152/api/finance/advice
```

Si la clé n'est pas configurée, vous verrez :
```
"Impossible de générer un conseil pour le moment. Vérifiez votre configuration."
```

Si tout fonctionne, vous recevrez un conseil financier généré par l'IA ! ??

---

## ?? Notes de sécurité

- ? **User Secrets** : Sécurisé pour le développement local
- ? **Variables d'environnement** : Sécurisé pour la production
- ? **appsettings.json** : NE JAMAIS stocker de secrets ici !
- ? **Code source** : NE JAMAIS hardcoder de secrets !

---

## ?? Dépannage

### "Clé API Gemini non configurée"

1. Vérifiez que la clé est configurée :
   ```bash
   dotnet user-secrets list
   ```

2. Si vide, configurez-la :
   ```bash
   dotnet user-secrets set "Gemini:ApiKey" "VOTRE_CLE"
   ```

### "Erreur de communication avec l'API Gemini"

1. Vérifiez que votre clé API est valide
2. Vérifiez votre connexion Internet
3. Consultez les logs dans la console pour plus de détails

### "UserSecretsId not found"

Réinitialisez User Secrets :
```bash
dotnet user-secrets init
dotnet user-secrets set "Gemini:ApiKey" "VOTRE_CLE"
```

---

## ?? Ressources

- [User Secrets dans ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Google Gemini API Documentation](https://ai.google.dev/docs)
- [Configuration dans ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
