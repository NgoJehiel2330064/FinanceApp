using FinanceApp.Data;
using FinanceApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// ============================================================================
// CR�ATION DU BUILDER
// ============================================================================
// WebApplication.CreateBuilder() :
// - Initialise la configuration (appsettings.json, variables d'environnement)
// - Configure le syst�me de logging
// - Pr�pare le conteneur d'injection de d�pendances (DI Container)
var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CONFIGURATION DES SERVICES (Dependency Injection Container)
// ============================================================================
// Tous les services enregistr�s ici seront disponibles pour l'injection
// dans les controllers, autres services, middleware, etc.

// ----------------------------------------------------------------------------
// CONTROLLERS
// ----------------------------------------------------------------------------
// AddControllers() : Enregistre les services n�cessaires pour les API Controllers
// - Routing (gestion des routes /api/...)
// - Model binding (conversion JSON ? objets C#)
// - Validation des donn�es
// - Formatters JSON
builder.Services.AddControllers();

// ----------------------------------------------------------------------------
// ENTITY FRAMEWORK CORE + POSTGRESQL
// ----------------------------------------------------------------------------
// AddDbContext<T>() : Enregistre le DbContext dans le conteneur DI
// Scope : SCOPED (une instance par requ�te HTTP)
// 
// POURQUOI SCOPED ?
// - Chaque requ�te HTTP a son propre DbContext
// - �vite les conflits entre requ�tes simultan�es
// - Le DbContext suit les changements (Change Tracking) pendant la requ�te
// - Automatiquement dispos� (ferm�) � la fin de la requ�te
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
    // 4. Npgsql utilise le protocole TCP/IP pour se connecter � localhost:5432
    // 5. Docker redirige la connexion vers le conteneur PostgreSQL
    // 
    // FLUX DE DONN�ES :
    // Program.cs ? DbContext ? Npgsql ? TCP/IP ? Docker (port 5432) ? PostgreSQL Container
    options.UseNpgsql(connectionString);

    // OPTIONS DE D�VELOPPEMENT (� commenter en production) :
    // EnableSensitiveDataLogging() : Affiche les valeurs des param�tres dans les logs SQL
    // Exemple : au lieu de "WHERE Id = @p0", affiche "WHERE Id = 5"
    // ?? DANGER en production : peut exposer des donn�es sensibles dans les logs
    // options.EnableSensitiveDataLogging();

    // EnableDetailedErrors() : Messages d'erreur plus d�taill�s
    // Utile pour debugger les probl�mes de mapping entre C# et SQL
    // options.EnableDetailedErrors();
});

// ----------------------------------------------------------------------------
// HTTP CLIENT FACTORY
// ----------------------------------------------------------------------------
// AddHttpClient() : Enregistre le HttpClientFactory dans le conteneur DI
// 
// POURQUOI HTTPCLIENTFACTORY ?
// 1. GESTION DU POOL DE CONNEXIONS :
//    - R�utilise les connexions TCP existantes
//    - �vite de cr�er/d�truire des sockets � chaque requ�te
//    - Am�liore les performances (moins d'overhead r�seau)
// 
// 2. �VITE L'�PUISEMENT DES PORTS (Socket Exhaustion) :
//    - Si vous cr�ez des HttpClient avec "new HttpClient()", vous risquez :
//      * D'�puiser les sockets disponibles (limite syst�me)
//      * Des TIME_WAIT sockets qui restent ouverts
//    - HttpClientFactory g�re automatiquement le cycle de vie
// 
// 3. GESTION AUTOMATIQUE DU DNS :
//    - Rafra�chit automatiquement les r�solutions DNS
//    - Important si l'IP du serveur change (load balancers, etc.)
// 
// 4. CONFIGURATION CENTRALIS�E :
//    - Timeout, headers par d�faut, retry policies, etc.
//    - Peut �tre configur� une fois pour toute l'application
builder.Services.AddHttpClient();

// ----------------------------------------------------------------------------
// SERVICES M�TIER (Business Services)
// ----------------------------------------------------------------------------
// AddScoped<Interface, Implementation>() : Enregistre un service avec scope SCOPED
// 
// SCOPED signifie :
// - Une nouvelle instance est cr��e pour chaque requ�te HTTP
// - La m�me instance est r�utilis�e dans toute la requ�te
// - Parfait pour les services qui gardent un �tat pendant la requ�te
// 
// EXEMPLE DE VIE D'UNE REQU�TE :
// 1. Requ�te HTTP arrive : POST /api/transactions
// 2. ASP.NET Core cr�e : ApplicationDbContext + GeminiService + TransactionsController
// 3. Le controller utilise ces instances pendant le traitement
// 4. � la fin de la requ�te, tout est automatiquement "disposed" (lib�r�)
// 5. Prochaine requ�te : nouvelles instances fra�ches
builder.Services.AddScoped<IGeminiService, GroqAIService>();
builder.Services.AddScoped<IAdvancedAnalyticsService, AdvancedAnalyticsService>();
builder.Services.AddScoped<INetWorthService, NetWorthService>();

// ----------------------------------------------------------------------------
// AUTHENTIFICATION JWT
// ----------------------------------------------------------------------------
// Configure l'authentification JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)
            ),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// ALTERNATIVE : AddSingleton vs AddScoped vs AddTransient
// 
// SINGLETON (AddSingleton) :
// - Une seule instance pour toute l'application
// - Cr��e au d�marrage, r�utilis�e pour toutes les requ�tes
// - ?? Doit �tre thread-safe (sans �tat ou avec locks)
// - Exemple : Configuration, Cache, Services sans �tat
// 
// SCOPED (AddScoped) :
// - Une instance par requ�te HTTP
// - Id�al pour DbContext et services qui gardent un �tat temporaire
// - Exemple : DbContext, UnitOfWork, Services m�tier
// 
// TRANSIENT (AddTransient) :
// - Une nouvelle instance � chaque injection
// - M�me dans la m�me requ�te, plusieurs instances diff�rentes
// - Exemple : Services l�gers sans �tat, Factories

// ----------------------------------------------------------------------------
// SWAGGER / OpenAPI
// ----------------------------------------------------------------------------
// Swagger : G�n�re automatiquement une documentation interactive de votre API
// Interface web : http://localhost:5000/swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ----------------------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing)
// ----------------------------------------------------------------------------
// CORS : Permet � un frontend sur un domaine diff�rent d'appeler votre API
// 
// POURQUOI CORS ?
// Par d�faut, les navigateurs bloquent les requ�tes entre domaines diff�rents :
// - Frontend : http://localhost:3000 (React/Vue/Angular)
// - Backend : http://localhost:5000 (API .NET)
// - Sans CORS, le navigateur refuse les appels API
// 
// POLITIQUE DE S�CURIT� :
// Le navigateur envoie d'abord une requ�te OPTIONS (preflight)
// pour v�rifier si le serveur autorise le domaine d'origine
builder.Services.AddCors(options =>
{
    // AddPolicy() : D�finit une politique CORS nomm�e
    options.AddPolicy("AllowFrontend", policy =>
    {
        // WithOrigins() : Liste des domaines autoris�s
        // Support pour plusieurs frameworks frontend sur diff�rents ports
        policy.WithOrigins(
                  "http://localhost:3000",     // Next.js, React (Create React App)
                  "http://localhost:3001",     // Next.js alternatif
                  "http://localhost:4200",     // Angular
                  "http://localhost:5173",     // Vite (React, Vue)
                  "http://localhost:8080"      // Vue CLI
              )
              // AllowAnyMethod() : Autorise GET, POST, PUT, DELETE, PATCH, etc.
              .AllowAnyMethod()
              // AllowAnyHeader() : Autorise tous les en-t�tes (Content-Type, Authorization, etc.)
              .AllowAnyHeader()
              // AllowCredentials() : Autorise l'envoi de cookies et credentials
              // N�cessaire si vous utilisez l'authentification bas�e sur cookies
              .AllowCredentials();
    });

    // POLITIQUE DE D�VELOPPEMENT : Autoriser tous les domaines (UNIQUEMENT EN D�VELOPPEMENT)
    // ?? DANGER en production : expose votre API � tous les sites web
    // Utilisez cette politique pendant le d�veloppement si vous avez des probl�mes CORS
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
// Build() : Construit l'application web avec tous les services configur�s
// � partir d'ici, on ne peut plus ajouter de services
var app = builder.Build();

// ============================================================================
// CONFIGURATION DU PIPELINE HTTP (Middleware)
// ============================================================================
// Le pipeline traite chaque requ�te HTTP dans l'ordre d�fini ici
// Chaque middleware peut :
// 1. Traiter la requ�te avant de passer au suivant
// 2. Passer au middleware suivant
// 3. Traiter la r�ponse en remontant
// 
// ORDRE D'EX�CUTION (IMPORTANT !) :
// Requ�te  ? Middleware 1 ? Middleware 2 ? Middleware 3 ? Controller
// R�ponse ? Middleware 1 ? Middleware 2 ? Middleware 3 ? Controller

// ----------------------------------------------------------------------------
// SWAGGER (uniquement en d�veloppement)
// ----------------------------------------------------------------------------
// app.Environment : D�tecte l'environnement (Development, Staging, Production)
// D�fini par la variable d'environnement ASPNETCORE_ENVIRONMENT
if (app.Environment.IsDevelopment())
{
    // UseSwagger() : Active l'endpoint /swagger/v1/swagger.json
    // G�n�re le fichier JSON d�crivant votre API (OpenAPI spec)
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
// Important pour la s�curit� en production
app.UseHttpsRedirection();

// ----------------------------------------------------------------------------
// CORS
// ----------------------------------------------------------------------------
// UseCors() : Active la politique CORS d�finie plus haut
// DOIT �tre avant UseAuthorization() et MapControllers()
// Traite les requ�tes preflight (OPTIONS) automatiquement
app.UseCors("AllowFrontend");

// ----------------------------------------------------------------------------
// AUTHENTIFICATION
// ----------------------------------------------------------------------------
app.UseAuthentication();

// ----------------------------------------------------------------------------
// AUTHORIZATION
// ----------------------------------------------------------------------------
// UseAuthorization() : Active le middleware d'autorisation
// V�rifie les attributs [Authorize] sur les controllers/actions
// Pour l'instant, pas d'authentification configur�e, mais on pr�pare le terrain
app.UseAuthorization();

// ----------------------------------------------------------------------------
// ROUTING - MAPPING DES CONTROLLERS
// ----------------------------------------------------------------------------
// MapControllers() : Scanne tous les controllers et mappe leurs routes
// Exemple : TransactionsController avec [Route("api/[controller]")]
// ? Routes cr��es : GET/POST/PUT/DELETE /api/transactions
// 
// COMMENT �A FONCTIONNE ?
// 1. Requ�te HTTP arrive : GET /api/transactions/5
// 2. Le router analyse la route
// 3. Trouve TransactionsController.GetTransaction(id: 5)
// 4. Le conteneur DI instancie le controller avec ses d�pendances
// 5. Appelle la m�thode GetTransaction(5)
// 6. La r�ponse remonte le pipeline
// 7. ASP.NET Core s�rialise le r�sultat en JSON
// 8. Retourne la r�ponse HTTP 200 avec le JSON
app.MapControllers();

// ============================================================================
// D�MARRAGE DE L'APPLICATION
// ============================================================================
// Run() : D�marre le serveur web Kestrel
// �coute sur les ports configur�s (g�n�ralement 5000 HTTP, 5001 HTTPS)
// Bloque le thread principal jusqu'� l'arr�t de l'application (Ctrl+C)
// 
// FLUX COMPLET D'UNE REQU�TE :
// 1. Client (Postman/Frontend) ? HTTP Request
// 2. Kestrel (serveur web) re�oit la requ�te
// 3. Pipeline HTTP (middleware) traite la requ�te
// 4. Router trouve le controller correspondant
// 5. DI Container instancie le controller + d�pendances
// 6. Action method s'ex�cute
// 7. DbContext ? Npgsql ? TCP/IP ? Docker ? PostgreSQL
// 8. PostgreSQL traite la requ�te SQL et retourne les donn�es
// 9. Docker ? TCP/IP ? Npgsql ? EF Core ? Controller
// 10. Controller retourne ActionResult
// 11. Pipeline remonte, serialise en JSON
// 12. Kestrel envoie HTTP Response au client
app.Run();
