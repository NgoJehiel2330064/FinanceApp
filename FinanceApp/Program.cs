using FinanceApp.Data;
using FinanceApp.Services;
using Microsoft.EntityFrameworkCore;

// ============================================================================
// CRÉATION DU BUILDER
// ============================================================================
// WebApplication.CreateBuilder() :
// - Initialise la configuration (appsettings.json, variables d'environnement)
// - Configure le système de logging
// - Prépare le conteneur d'injection de dépendances (DI Container)
var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CONFIGURATION DES SERVICES (Dependency Injection Container)
// ============================================================================
// Tous les services enregistrés ici seront disponibles pour l'injection
// dans les controllers, autres services, middleware, etc.

// ----------------------------------------------------------------------------
// CONTROLLERS
// ----------------------------------------------------------------------------
// AddControllers() : Enregistre les services nécessaires pour les API Controllers
// - Routing (gestion des routes /api/...)
// - Model binding (conversion JSON ? objets C#)
// - Validation des données
// - Formatters JSON
builder.Services.AddControllers();

// ----------------------------------------------------------------------------
// ENTITY FRAMEWORK CORE + POSTGRESQL
// ----------------------------------------------------------------------------
// AddDbContext<T>() : Enregistre le DbContext dans le conteneur DI
// Scope : SCOPED (une instance par requête HTTP)
// 
// POURQUOI SCOPED ?
// - Chaque requête HTTP a son propre DbContext
// - Évite les conflits entre requêtes simultanées
// - Le DbContext suit les changements (Change Tracking) pendant la requête
// - Automatiquement disposé (fermé) à la fin de la requête
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // GetConnectionString() : Lit "ConnectionStrings:DefaultConnection" depuis appsettings.json
    // Contenu : "Host=localhost;Port=5432;Database=finance_db;Username=postgres;Password=admin123"
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // UseNpgsql() : Configure EF Core pour utiliser PostgreSQL
    // 
    // COMMUNICATION AVEC DOCKER :
    // 1. Votre app ASP.NET Core tourne sur Windows (localhost)
    // 2. PostgreSQL tourne dans un conteneur Docker
    // 3. Docker expose le port 5432 du conteneur vers votre Windows (localhost:5432)
    // 4. Npgsql utilise le protocole TCP/IP pour se connecter à localhost:5432
    // 5. Docker redirige la connexion vers le conteneur PostgreSQL
    // 
    // FLUX DE DONNÉES :
    // Program.cs ? DbContext ? Npgsql ? TCP/IP ? Docker (port 5432) ? PostgreSQL Container
    options.UseNpgsql(connectionString);

    // OPTIONS DE DÉVELOPPEMENT (à commenter en production) :
    // EnableSensitiveDataLogging() : Affiche les valeurs des paramètres dans les logs SQL
    // Exemple : au lieu de "WHERE Id = @p0", affiche "WHERE Id = 5"
    // ?? DANGER en production : peut exposer des données sensibles dans les logs
    // options.EnableSensitiveDataLogging();

    // EnableDetailedErrors() : Messages d'erreur plus détaillés
    // Utile pour debugger les problèmes de mapping entre C# et SQL
    // options.EnableDetailedErrors();
});

// ----------------------------------------------------------------------------
// HTTP CLIENT FACTORY
// ----------------------------------------------------------------------------
// AddHttpClient() : Enregistre le HttpClientFactory dans le conteneur DI
// 
// POURQUOI HTTPCLIENTFACTORY ?
// 1. GESTION DU POOL DE CONNEXIONS :
//    - Réutilise les connexions TCP existantes
//    - Évite de créer/détruire des sockets à chaque requête
//    - Améliore les performances (moins d'overhead réseau)
// 
// 2. ÉVITE L'ÉPUISEMENT DES PORTS (Socket Exhaustion) :
//    - Si vous créez des HttpClient avec "new HttpClient()", vous risquez :
//      * D'épuiser les sockets disponibles (limite système)
//      * Des TIME_WAIT sockets qui restent ouverts
//    - HttpClientFactory gère automatiquement le cycle de vie
// 
// 3. GESTION AUTOMATIQUE DU DNS :
//    - Rafraîchit automatiquement les résolutions DNS
//    - Important si l'IP du serveur change (load balancers, etc.)
// 
// 4. CONFIGURATION CENTRALISÉE :
//    - Timeout, headers par défaut, retry policies, etc.
//    - Peut être configuré une fois pour toute l'application
builder.Services.AddHttpClient();

// ----------------------------------------------------------------------------
// SERVICES MÉTIER (Business Services)
// ----------------------------------------------------------------------------
// AddScoped<Interface, Implementation>() : Enregistre un service avec scope SCOPED
// 
// SCOPED signifie :
// - Une nouvelle instance est créée pour chaque requête HTTP
// - La même instance est réutilisée dans toute la requête
// - Parfait pour les services qui gardent un état pendant la requête
// 
// EXEMPLE DE VIE D'UNE REQUÊTE :
// 1. Requête HTTP arrive : POST /api/transactions
// 2. ASP.NET Core crée : ApplicationDbContext + GeminiService + TransactionsController
// 3. Le controller utilise ces instances pendant le traitement
// 4. À la fin de la requête, tout est automatiquement "disposed" (libéré)
// 5. Prochaine requête : nouvelles instances fraîches
builder.Services.AddScoped<IGeminiService, GeminiService>();

// ALTERNATIVE : AddSingleton vs AddScoped vs AddTransient
// 
// SINGLETON (AddSingleton) :
// - Une seule instance pour toute l'application
// - Créée au démarrage, réutilisée pour toutes les requêtes
// - ?? Doit être thread-safe (sans état ou avec locks)
// - Exemple : Configuration, Cache, Services sans état
// 
// SCOPED (AddScoped) :
// - Une instance par requête HTTP
// - Idéal pour DbContext et services qui gardent un état temporaire
// - Exemple : DbContext, UnitOfWork, Services métier
// 
// TRANSIENT (AddTransient) :
// - Une nouvelle instance à chaque injection
// - Même dans la même requête, plusieurs instances différentes
// - Exemple : Services légers sans état, Factories

// ----------------------------------------------------------------------------
// SWAGGER / OpenAPI
// ----------------------------------------------------------------------------
// Swagger : Génère automatiquement une documentation interactive de votre API
// Interface web : http://localhost:5000/swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ----------------------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing)
// ----------------------------------------------------------------------------
// CORS : Permet à un frontend sur un domaine différent d'appeler votre API
// 
// POURQUOI CORS ?
// Par défaut, les navigateurs bloquent les requêtes entre domaines différents :
// - Frontend : http://localhost:3000 (React/Vue/Angular)
// - Backend : http://localhost:5000 (API .NET)
// - Sans CORS, le navigateur refuse les appels API
// 
// POLITIQUE DE SÉCURITÉ :
// Le navigateur envoie d'abord une requête OPTIONS (preflight)
// pour vérifier si le serveur autorise le domaine d'origine
builder.Services.AddCors(options =>
{
    // AddPolicy() : Définit une politique CORS nommée
    options.AddPolicy("AllowFrontend", policy =>
    {
        // WithOrigins() : Liste des domaines autorisés
        // Support pour plusieurs frameworks frontend sur différents ports
        policy.WithOrigins(
                  "http://localhost:3000",     // Next.js, React (Create React App)
                  "http://localhost:3001",     // Next.js alternatif
                  "http://localhost:4200",     // Angular
                  "http://localhost:5173",     // Vite (React, Vue)
                  "http://localhost:8080"      // Vue CLI
              )
              // AllowAnyMethod() : Autorise GET, POST, PUT, DELETE, PATCH, etc.
              .AllowAnyMethod()
              // AllowAnyHeader() : Autorise tous les en-têtes (Content-Type, Authorization, etc.)
              .AllowAnyHeader()
              // AllowCredentials() : Autorise l'envoi de cookies et credentials
              // Nécessaire si vous utilisez l'authentification basée sur cookies
              .AllowCredentials();
    });

    // POLITIQUE DE DÉVELOPPEMENT : Autoriser tous les domaines (UNIQUEMENT EN DÉVELOPPEMENT)
    // ?? DANGER en production : expose votre API à tous les sites web
    // Utilisez cette politique pendant le développement si vous avez des problèmes CORS
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================================================
// BUILD DE L'APPLICATION
// ============================================================================
// Build() : Construit l'application web avec tous les services configurés
// À partir d'ici, on ne peut plus ajouter de services
var app = builder.Build();

// ============================================================================
// CONFIGURATION DU PIPELINE HTTP (Middleware)
// ============================================================================
// Le pipeline traite chaque requête HTTP dans l'ordre défini ici
// Chaque middleware peut :
// 1. Traiter la requête avant de passer au suivant
// 2. Passer au middleware suivant
// 3. Traiter la réponse en remontant
// 
// ORDRE D'EXÉCUTION (IMPORTANT !) :
// Requête  ? Middleware 1 ? Middleware 2 ? Middleware 3 ? Controller
// Réponse ? Middleware 1 ? Middleware 2 ? Middleware 3 ? Controller

// ----------------------------------------------------------------------------
// SWAGGER (uniquement en développement)
// ----------------------------------------------------------------------------
// app.Environment : Détecte l'environnement (Development, Staging, Production)
// Défini par la variable d'environnement ASPNETCORE_ENVIRONMENT
if (app.Environment.IsDevelopment())
{
    // UseSwagger() : Active l'endpoint /swagger/v1/swagger.json
    // Génère le fichier JSON décrivant votre API (OpenAPI spec)
    app.UseSwagger();

    // UseSwaggerUI() : Active l'interface web interactive /swagger
    // Permet de tester votre API directement dans le navigateur
    app.UseSwaggerUI();
}

// ----------------------------------------------------------------------------
// HTTPS REDIRECTION
// ----------------------------------------------------------------------------
// UseHttpsRedirection() : Redirige automatiquement HTTP ? HTTPS
// Exemple : http://localhost:5000 ? https://localhost:5001
// Important pour la sécurité en production
app.UseHttpsRedirection();

// ----------------------------------------------------------------------------
// CORS
// ----------------------------------------------------------------------------
// UseCors() : Active la politique CORS définie plus haut
// DOIT être avant UseAuthorization() et MapControllers()
// Traite les requêtes preflight (OPTIONS) automatiquement
app.UseCors("AllowFrontend");

// ----------------------------------------------------------------------------
// AUTHORIZATION
// ----------------------------------------------------------------------------
// UseAuthorization() : Active le middleware d'autorisation
// Vérifie les attributs [Authorize] sur les controllers/actions
// Pour l'instant, pas d'authentification configurée, mais on prépare le terrain
app.UseAuthorization();

// ----------------------------------------------------------------------------
// ROUTING - MAPPING DES CONTROLLERS
// ----------------------------------------------------------------------------
// MapControllers() : Scanne tous les controllers et mappe leurs routes
// Exemple : TransactionsController avec [Route("api/[controller]")]
// ? Routes créées : GET/POST/PUT/DELETE /api/transactions
// 
// COMMENT ÇA FONCTIONNE ?
// 1. Requête HTTP arrive : GET /api/transactions/5
// 2. Le router analyse la route
// 3. Trouve TransactionsController.GetTransaction(id: 5)
// 4. Le conteneur DI instancie le controller avec ses dépendances
// 5. Appelle la méthode GetTransaction(5)
// 6. La réponse remonte le pipeline
// 7. ASP.NET Core sérialise le résultat en JSON
// 8. Retourne la réponse HTTP 200 avec le JSON
app.MapControllers();

// ============================================================================
// DÉMARRAGE DE L'APPLICATION
// ============================================================================
// Run() : Démarre le serveur web Kestrel
// Écoute sur les ports configurés (généralement 5000 HTTP, 5001 HTTPS)
// Bloque le thread principal jusqu'à l'arrêt de l'application (Ctrl+C)
// 
// FLUX COMPLET D'UNE REQUÊTE :
// 1. Client (Postman/Frontend) ? HTTP Request
// 2. Kestrel (serveur web) reçoit la requête
// 3. Pipeline HTTP (middleware) traite la requête
// 4. Router trouve le controller correspondant
// 5. DI Container instancie le controller + dépendances
// 6. Action method s'exécute
// 7. DbContext ? Npgsql ? TCP/IP ? Docker ? PostgreSQL
// 8. PostgreSQL traite la requête SQL et retourne les données
// 9. Docker ? TCP/IP ? Npgsql ? EF Core ? Controller
// 10. Controller retourne ActionResult
// 11. Pipeline remonte, serialise en JSON
// 12. Kestrel envoie HTTP Response au client
app.Run();
