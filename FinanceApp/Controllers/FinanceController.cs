using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceApp.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FinanceApp.Controllers;

/// <summary>
/// Controller API REST pour les fonctionnalit�s financi�res avanc�es (IA)
/// </summary>
/// <remarks>
/// Ce controller g�re les endpoints li�s � l'intelligence artificielle :
/// - Conseils financiers personnalis�s
/// - Analyse des habitudes de d�penses
/// - Pr�dictions budg�taires
/// 
/// [ApiController] : Active les comportements API (validation auto, binding auto, etc.)
/// [Route("api/[controller]")] : Route de base = /api/finance
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<FinanceController> _logger;

    /// <summary>
    /// Constructeur avec injection de d�pendances
    /// </summary>
    /// <param name="geminiService">Service IA Gemini pour les analyses</param>
    /// <param name="logger">Service de logging</param>
    /// <remarks>
    /// INJECTION DE D�PENDANCES :
    /// 
    /// Quand une requ�te HTTP arrive sur /api/finance/advice :
    /// 1. ASP.NET Core identifie le controller : FinanceController
    /// 2. Regarde les param�tres du constructeur : IGeminiService, ILogger
    /// 3. Cherche dans le conteneur DI (configur� dans Program.cs)
    /// 4. Trouve les impl�mentations enregistr�es :
    ///    - IGeminiService ? GeminiService (Scoped)
    ///    - ILogger ? Service de logging int�gr� (Singleton)
    /// 5. Instancie le controller avec ces d�pendances
    /// 6. Appelle la m�thode d'action (GetFinancialAdvice)
    /// 7. � la fin de la requ�te, dispose les services Scoped
    /// </remarks>
    public FinanceController(IGeminiService geminiService, ILogger<FinanceController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/finance/advice
    /// Obtient un conseil financier personnalis� bas� sur l'analyse IA des transactions
    /// </summary>
    /// <returns>Conseil financier g�n�r� par Gemini (15 mots max)</returns>
    /// <remarks>
    /// FLUX COMPLET DE LA DONN�E (Request ? Database ? IA ? Response) :
    /// 
    /// ???????????????????????????????????????????????????????????????????
    /// ? 1. CLIENT (Frontend React/Postman)                             ?
    /// ?    ? Envoie : GET http://localhost:5000/api/finance/advice     ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ? HTTP Request
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 2. KESTREL (Serveur Web ASP.NET Core)                          ?
    /// ?    ? Re�oit la requ�te HTTP                                     ?
    /// ?    ? Passe au pipeline middleware                               ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ?
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 3. MIDDLEWARE PIPELINE (Program.cs)                             ?
    /// ?    ? CORS : V�rifie l'origine (localhost:3000 autoris�)        ?
    /// ?    ? Authorization : V�rifie les permissions (pour l'instant ?) ?
    /// ?    ? Routing : Trouve le controller correspondant              ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ?
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 4. DI CONTAINER (Dependency Injection)                          ?
    /// ?    ? Instancie FinanceController                                ?
    /// ?    ? Injecte GeminiService (Scoped - nouvelle instance)        ?
    /// ?    ? Injecte ApplicationDbContext (Scoped)                      ?
    /// ?    ? Injecte ILogger (Singleton)                                ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ?
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 5. CONTROLLER (FinanceController.GetFinancialAdvice)            ?
    /// ?    ? Log : "Demande de conseil financier re�ue"                ?
    /// ?    ? Appelle : _geminiService.GetFinancialAdvice()             ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ?
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 6. GEMINI SERVICE (GetFinancialAdvice)                          ?
    /// ?    ? Appelle : _context.Transactions.ToListAsync()             ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ?
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 7. ENTITY FRAMEWORK CORE (ApplicationDbContext)                 ?
    /// ?    ? G�n�re le SQL : SELECT * FROM "Transactions"              ?
    /// ?    ? Passe � Npgsql                                             ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ?
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 8. NPGSQL (Driver PostgreSQL)                                   ?
    /// ?    ? Encode la requ�te en protocole PostgreSQL                  ?
    /// ?    ? Envoie via TCP/IP � localhost:5432                        ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ? TCP/IP
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 9. DOCKER                                                       ?
    /// ?    ? Intercepte le port 5432                                    ?
    /// ?    ? Redirige vers le conteneur postgres_db                    ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ?
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 10. POSTGRESQL (Conteneur Docker)                               ?
    /// ?    ? Ex�cute la requ�te SQL                                     ?
    /// ?    ? Retourne les lignes de la table "Transactions"            ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ? R�sultats SQL
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 11. FLUX RETOUR : PostgreSQL ? Docker ? Npgsql ? EF Core       ?
    /// ?    ? EF Core mappe les lignes SQL en objets Transaction        ?
    /// ?    ? Retourne List<Transaction> au GeminiService               ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ?
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 12. GEMINI SERVICE (Analyse des donn�es)                        ?
    /// ?    ? Calcule : revenus, d�penses, balance, top cat�gorie       ?
    /// ?    ? Construit un prompt pour Gemini                            ?
    /// ?    ? Appelle : CallGeminiApiAsync()                             ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ?
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 13. APPEL API GEMINI (via HttpClient)                           ?
    /// ?    ? POST https://generativelanguage.googleapis.com/...        ?
    /// ?    ? Body JSON : { contents: [{ parts: [{ text: prompt }] }] } ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ? HTTPS Request
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 14. SERVEURS GOOGLE (API Gemini)                                ?
    /// ?    ? Re�oit le prompt                                           ?
    /// ?    ? Ex�cute le mod�le d'IA (gemini-1.5-flash)                 ?
    /// ?    ? G�n�re un conseil financier (15 mots max)                 ?
    /// ?    ? Retourne JSON : { candidates: [{ content: ... }] }        ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ? HTTPS Response
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 15. GEMINI SERVICE (Parse la r�ponse)                           ?
    /// ?    ? Parse le JSON                                              ?
    /// ?    ? Extrait le texte g�n�r�                                    ?
    /// ?    ? Retourne le conseil au Controller                         ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ?
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 16. CONTROLLER (GetFinancialAdvice)                             ?
    /// ?    ? Re�oit le conseil du GeminiService                         ?
    /// ?    ? Retourne : Ok(new { advice = "..." })                     ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ?
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 17. MIDDLEWARE PIPELINE (Remont�e)                              ?
    /// ?    ? S�rialise l'objet en JSON                                  ?
    /// ?    ? Ajoute les headers CORS                                    ?
    /// ?    ? G�n�re la r�ponse HTTP 200 OK                             ?
    /// ???????????????????????????????????????????????????????????????????
    ///                       ? HTTP Response
    ///                       ?
    /// ???????????????????????????????????????????????????????????????????
    /// ? 18. CLIENT (Frontend React/Postman)                             ?
    /// ?    ? Re�oit : { "advice": "R�duisez vos d�penses..." }        ?
    /// ?    ? Affiche le conseil � l'utilisateur                        ?
    /// ???????????????????????????????????????????????????????????????????
    /// 
    /// EXEMPLE DE R�PONSE :
    /// {
    ///   "advice": "R�duisez vos d�penses en alimentation de 20% pour �conomiser 100� mensuels."
    /// }
    /// 
    /// CODES HTTP POSSIBLES :
    /// - 200 OK : Conseil g�n�r� avec succ�s
    /// - 500 Internal Server Error : Erreur serveur (base de donn�es, API Gemini, etc.)
    /// </remarks>
    [HttpGet("advice")]
    public async Task<ActionResult<object>> GetFinancialAdvice([FromQuery] int userId)
    {
        try
        {
            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            if (userId <= 0)
            {
                userId = tokenUserId.Value;
            }
            else if (userId != tokenUserId.Value)
            {
                return Forbid();
            }

            // Log de la requ?te entrante
            _logger.LogInformation("Demande de conseil financier re?ue pour l'utilisateur {UserId}", userId);

            // ================================================================
            // APPEL DU SERVICE GEMINI
            // ================================================================
            // GetFinancialAdvice() d?clenche tout le flux d?crit ci-dessus :
            // 1. R?cup?ration des transactions depuis PostgreSQL
            // 2. Analyse des donn?es financi?res
            // 3. Construction du prompt
            // 4. Appel ? l'API Gemini
            // 5. Parsing de la r?ponse
            // 6. Retour du conseil
            var advice = await _geminiService.GetFinancialAdvice(userId);

            _logger.LogInformation("Conseil g�n�r� : {Advice}", advice);

            // ================================================================
            // RETOUR DE LA R�PONSE
            // ================================================================
            // Ok() : Retourne HTTP 200 avec un objet JSON
            // L'objet anonyme { advice = "..." } est automatiquement s�rialis� en JSON
            // 
            // R�ponse HTTP :
            // Status: 200 OK
            // Content-Type: application/json
            // Body: { "advice": "texte du conseil..." }
            return Ok(new { advice });
        }
        catch (Exception ex)
        {
            // En cas d'erreur (base de donn�es inaccessible, API Gemini down, etc.)
            _logger.LogError(ex, "Erreur lors de la g�n�ration du conseil financier");

            // StatusCode(500) : Retourne HTTP 500 Internal Server Error
            // Avec un message d'erreur pour le frontend
            return StatusCode(500, new 
            { 
                error = "Impossible de g�n�rer un conseil pour le moment.",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// POST /api/finance/suggest-category
    /// Sugg�re une cat�gorie pour une transaction bas�e sur sa description et son montant
    /// </summary>
    /// <param name="request">Donn�es de la transaction (description + montant)</param>
    /// <returns>Nom de la cat�gorie sugg�r�e par l'IA</returns>
    /// <remarks>
    /// EXEMPLE DE REQU�TE :
    /// POST /api/finance/suggest-category
    /// {
    ///   "description": "Courses Lidl",
    ///   "amount": 45.80
    /// }
    /// 
    /// EXEMPLE DE R�PONSE :
    /// {
    ///   "category": "Alimentation"
    /// }
    /// 
    /// CAT�GORIES POSSIBLES :
    /// - Alimentation
    /// - Transport
    /// - Logement
    /// - Loisirs
    /// - Sant�
    /// - �ducation
    /// - V�tements
    /// - Technologie
    /// - Services
    /// - Autres
    /// </remarks>
    [HttpPost("suggest-category")]
    public async Task<ActionResult<object>> SuggestCategory([FromQuery] int userId, [FromBody] CategorySuggestionRequest request)
    {
        try
        {
            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            if (userId <= 0)
            {
                userId = tokenUserId.Value;
            }
            else if (userId != tokenUserId.Value)
            {
                return Forbid();
            }

            _logger.LogInformation("Demande de suggestion de categorie pour l'utilisateur {UserId} : '{Description}', {Amount}", userId, request.Description, request.Amount);

            // Validation
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { error = "La description est requise" });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new { error = "Le montant doit �tre positif" });
            }

            // Appel au service IA
            var category = await _geminiService.SuggestCategoryAsync(userId, request.Description, request.Amount);

            _logger.LogInformation("Cat�gorie sugg�r�e : {Category}", category);

            return Ok(new { category });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suggestion de cat�gorie");
            return StatusCode(500, new 
            { 
                error = "Impossible de sugg�rer une cat�gorie pour le moment.",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// GET /api/finance/summary
    /// G�n�re un r�sum� financier personnalis� pour une p�riode donn�e
    /// </summary>
    /// <param name="startDate">Date de d�but (format ISO 8601)</param>
    /// <param name="endDate">Date de fin (format ISO 8601)</param>
    /// <returns>R�sum� financier g�n�r� par l'IA</returns>
    /// <remarks>
    /// EXEMPLE DE REQU�TE :
    /// GET /api/finance/summary?startDate=2025-01-01&endDate=2025-01-31
    /// 
    /// EXEMPLE DE R�PONSE :
    /// {
    ///   "summary": "En janvier, vous avez �conomis� 15% de vos revenus. Vos d�penses en Alimentation sont sous contr�le. Continuez ainsi !"
    /// }
    /// 
    /// UTILIT� :
    /// - Rapport mensuel automatique
    /// - Dashboard avec KPIs
    /// - Analyse de p�riode personnalis�e
    /// </remarks>
    [HttpGet("summary")]
    public async Task<ActionResult<object>> GetFinancialSummary(
        [FromQuery] int userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            if (userId <= 0)
            {
                userId = tokenUserId.Value;
            }
            else if (userId != tokenUserId.Value)
            {
                return Forbid();
            }

            _logger.LogInformation(
                "Demande de résumé financier : {StartDate} - {EndDate}",
                startDate,
                endDate
            );

            // Validation améliorée avec plus de tolérance
            if (startDate >= endDate)
            {
                _logger.LogWarning("Validation échouée: startDate >= endDate");
                return BadRequest(new { error = "La date de début doit être antérieure à la date de fin" });
            }

            // Convertir les dates en UTC pour comparaison précise
            var nowUtc = DateTime.UtcNow;
            var endDateUtc = endDate.ToUniversalTime();
            
            // Tolérance de 10 minutes au lieu de 1 pour gérer les désynchronisations d'horloge
            if (endDateUtc > nowUtc.AddMinutes(10))
            {
                _logger.LogWarning(
                    "Validation échouée: endDate dans le futur. Now={Now}, EndDate={EndDate}, Différence={Diff} minutes",
                    nowUtc,
                    endDateUtc,
                    (endDateUtc - nowUtc).TotalMinutes
                );
                return BadRequest(new { 
                    error = "La date de fin ne peut pas être dans le futur",
                    details = $"Date de fin reçue: {endDateUtc:O}, Heure serveur: {nowUtc:O}"
                });
            }

            // Appel au service IA
            var summary = await _geminiService.GenerateFinancialSummaryAsync(userId, startDate, endDate);

            _logger.LogInformation("Résumé généré avec succès: {Summary}", summary?.Substring(0, Math.Min(50, summary?.Length ?? 0)));

            return Ok(new { summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération du résumé financier");
            return StatusCode(500, new 
            { 
                error = "Impossible de générer un résumé pour le moment.",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// POST /api/finance/chat
    /// Chat conversationnel IA avec contexte utilisateur
    /// </summary>
    [HttpPost("chat")]
    public async Task<ActionResult<object>> Chat([FromBody] ChatRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Le message est requis" });
            }

            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            var userId = request.UserId <= 0 ? tokenUserId.Value : request.UserId;
            if (userId != tokenUserId.Value)
            {
                return Forbid();
            }

            var context = request.Context ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(request.Page))
            {
                context = $"Page: {request.Page}\n" + context;
            }

            var reply = await _geminiService.GetChatResponseAsync(userId, request.Message, context);
            return Ok(new { reply });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chat IA");
            return StatusCode(500, new { error = "Impossible de traiter votre demande pour le moment." });
        }
    }

    /// <summary>
    /// GET /api/finance/anomalies
    /// D�tecte les anomalies dans les transactions (d�penses inhabituelles)
    /// </summary>
    /// <returns>Liste des anomalies d�tect�es</returns>
    /// <remarks>
    /// EXEMPLE DE R�PONSE :
    /// {
    ///   "anomalies": [
    ///     "D�pense inhabituelle : 450� en Loisirs le 15/02 (moyenne: 80�, �cart: +462%)",
    ///     "Pic de d�penses Alimentation : +40% par rapport au mois dernier"
    ///   ]
    /// }
    /// 
    /// ALGORITHME :
    /// - Calcule la moyenne par cat�gorie
    /// - D�tecte les �carts > 50% de la moyenne
    /// - Enrichit avec des alertes g�n�r�es par Gemini
    /// 
    /// UTILIT� :
    /// - D�tection de fraude
    /// - Alertes automatiques
    /// - Suivi des habitudes de d�penses
    /// </remarks>
    [HttpGet("anomalies")]
    public async Task<ActionResult<object>> DetectAnomalies([FromQuery] int userId)
    {
        try
        {
            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            if (userId <= 0)
            {
                userId = tokenUserId.Value;
            }
            else if (userId != tokenUserId.Value)
            {
                return Forbid();
            }

            _logger.LogInformation("Demande de d?tection d'anomalies pour l'utilisateur {UserId}", userId);

            // Appel au service IA
            var anomalies = await _geminiService.DetectAnomaliesAsync(userId);

            _logger.LogInformation("D�tection termin�e : {Count} anomalie(s)", anomalies.Count);

            return Ok(new { anomalies });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la d�tection d'anomalies");
            return StatusCode(500, new 
            { 
                error = "Impossible de d�tecter les anomalies pour le moment.",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// GET /api/finance/predict
    /// Pr�dit le budget futur bas� sur les tendances historiques
    /// </summary>
    /// <param name="monthsAhead">Nombre de mois � pr�dire (1-12)</param>
    /// <returns>Pr�diction budg�taire g�n�r�e par l'IA</returns>
    /// <remarks>
    /// EXEMPLE DE REQU�TE :
    /// GET /api/finance/predict?monthsAhead=3
    /// 
    /// EXEMPLE DE R�PONSE :
    /// {
    ///   "prediction": "Bas� sur vos habitudes, vous �conomiserez environ 600� dans 3 mois. Maintenez vos efforts d'�pargne !"
    /// }
    /// 
    /// ALGORITHME :
    /// - Analyse des 3 derniers mois
    /// - Calcul des moyennes et tendances
    /// - Pr�diction par Gemini avec contexte
    /// 
    /// UTILIT� :
    /// - Planification budg�taire
    /// - Objectifs d'�pargne
    /// - Motivation financi�re
    /// </remarks>
    [HttpGet("predict")]
    public async Task<ActionResult<object>> PredictBudget([FromQuery] int userId, [FromQuery] int monthsAhead = 3)
    {
        try
        {
            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            if (userId <= 0)
            {
                userId = tokenUserId.Value;
            }
            else if (userId != tokenUserId.Value)
            {
                return Forbid();
            }

            _logger.LogInformation("Demande de pr?diction de budget pour l'utilisateur {UserId} : {Months} mois", userId, monthsAhead);

            // Validation
            if (monthsAhead <= 0 || monthsAhead > 12)
            {
                return BadRequest(new { error = "Le nombre de mois doit ?tre entre 1 et 12" });
            }

            // Appel au service IA
            var prediction = await _geminiService.PredictBudgetAsync(userId, monthsAhead);

            _logger.LogInformation("Pr�diction g�n�r�e : {Prediction}", prediction);

            return Ok(new { prediction });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la pr�diction de budget");
            return StatusCode(500, new 
            { 
                error = "Impossible de g�n�rer une pr�diction pour le moment.",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// GET /api/finance/portfolio-insights
    /// Analyse le patrimoine global et g�n�re des insights strat�giques personnalis�s
    /// </summary>
    /// <returns>3 insights patrimoniaux g�n�r�s par l'IA</returns>
    /// <remarks>
    /// EXEMPLE DE REQU�TE :
    /// GET /api/finance/portfolio-insights
    /// 
    /// EXEMPLE DE R�PONSE :
    /// {
    ///   "insights": [
    ///     "Votre patrimoine est fortement concentr� en immobilier (70%), ce qui limite la liquidit� et la diversification.",
    ///     "Vos revenus mensuels (3200 CAD) repr�sentent seulement 0.7% de la valeur de vos actifs, un ratio faible.",
    ///     "Seulement 20% de votre patrimoine est investi dans des actifs productifs (investissements)."
    ///   ]
    /// }
    /// 
    /// ALGORITHME :
    /// 1. R�cup�re tous les assets et transactions
    /// 2. Calcule :
    ///    - Valeur totale et r�partition par type
    ///    - Revenus/d�penses mensuels moyens
    ///    - Ratio revenus/patrimoine
    ///    - % d'assets productifs
    /// 3. G�n�re 3 insights via Gemini
    /// 
    /// UTILIT� :
    /// - Dashboard patrimonial
    /// - Analyse de diversification
    /// - Conseils strat�giques
    /// - Suivi de la structure du patrimoine
    /// </remarks>
    [HttpGet("portfolio-insights")]
    public async Task<ActionResult<object>> GetPortfolioInsights([FromQuery] int userId)
    {
        try
        {
            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            if (userId <= 0)
            {
                userId = tokenUserId.Value;
            }
            else if (userId != tokenUserId.Value)
            {
                return Forbid();
            }

            _logger.LogInformation("Demande d'insights patrimoniaux pour l'utilisateur {UserId}", userId);

            // Appel au service IA
            var insights = await _geminiService.GetPortfolioInsightsAsync(userId);

            _logger.LogInformation("Insights g�n�r�s : {Count} insight(s)", insights.Count);

            return Ok(new { insights });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la g�n�ration d'insights patrimoniaux");
            return StatusCode(500, new 
            { 
                error = "Impossible de g�n�rer des insights pour le moment.",
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// GET /api/finance/spending-patterns
    /// Analyse les patterns de dépenses de l'utilisateur
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <param name="monthsToAnalyze">Nombre de mois à analyser (défaut: 3)</param>
    /// <returns>Patterns de dépenses avec catégories, tendances, etc.</returns>
    [HttpGet("spending-patterns")]
    public async Task<ActionResult<object>> GetSpendingPatterns(
        [FromQuery] int userId,
        [FromQuery] int monthsToAnalyze = 3)
    {
        try
        {
            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            if (userId <= 0)
            {
                userId = tokenUserId.Value;
            }
            else if (userId != tokenUserId.Value)
            {
                return Forbid();
            }

            _logger.LogInformation("Analyse des patterns de dépenses pour l'utilisateur {UserId}", userId);

            var advancedAnalytics = HttpContext.RequestServices.GetRequiredService<IAdvancedAnalyticsService>();
            var patterns = await advancedAnalytics.AnalyzeSpendingPatternsAsync(userId, monthsToAnalyze);

            _logger.LogInformation("Patterns analysés : {TotalTransactions} transactions", patterns.TotalTransactions);

            return Ok(new
            {
                patterns.TotalTransactions,
                patterns.TotalSpent,
                patterns.AverageMonthlySpending,
                patterns.HighestSpendingMonth,
                patterns.LowestSpendingMonth,
                patterns.SpendingVariance,
                patterns.TrendDirection,
                patterns.MostSpentCategory,
                Categories = patterns.Categories.Select(c => new
                {
                    c.Value.Category,
                    c.Value.TotalSpent,
                    c.Value.TransactionCount,
                    c.Value.AverageTransaction,
                    c.Value.Percentage,
                    c.Value.IsRecurring
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse des patterns de dépenses");
            return StatusCode(500, new
            {
                error = "Impossible d'analyser les patterns de dépenses.",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// GET /api/finance/smart-anomalies
    /// Détecte les anomalies avec analyse avancée (amélioration de /anomalies)
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <returns>Rapport d'anomalies détectées avec patterns</returns>
    [HttpGet("smart-anomalies")]
    public async Task<ActionResult<object>> GetSmartAnomalies([FromQuery] int userId)
    {
        try
        {
            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            if (userId <= 0)
            {
                userId = tokenUserId.Value;
            }
            else if (userId != tokenUserId.Value)
            {
                return Forbid();
            }

            _logger.LogInformation("Détection intelligente d'anomalies pour l'utilisateur {UserId}", userId);

            var advancedAnalytics = HttpContext.RequestServices.GetRequiredService<IAdvancedAnalyticsService>();
            var anomalies = await advancedAnalytics.DetectAnomaliesAsync(userId);

            _logger.LogInformation("Anomalies détectées : {Count}", anomalies.TotalAnomalies);

            return Ok(new
            {
                anomalies.TotalAnomalies,
                anomalies.HighSeverityCount,
                anomalies.MediumSeverityCount,
                anomalies.LowSeverityCount,
                anomalies.HasCriticalAnomalies,
                anomalies.Anomalies
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la détection intelligente d'anomalies");
            return StatusCode(500, new
            {
                error = "Impossible de détecter les anomalies.",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// GET /api/finance/recommendations
    /// Génère des recommandations personnalisées basées sur les patterns et anomalies
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <returns>Liste de recommandations prioritaires</returns>
    [HttpGet("recommendations")]
    public async Task<ActionResult<object>> GetRecommendations([FromQuery] int userId)
    {
        try
        {
            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new { message = "Token invalide" });
            }

            if (userId <= 0)
            {
                userId = tokenUserId.Value;
            }
            else if (userId != tokenUserId.Value)
            {
                return Forbid();
            }

            _logger.LogInformation("Génération des recommandations pour l'utilisateur {UserId}", userId);

            var advancedAnalytics = HttpContext.RequestServices.GetRequiredService<IAdvancedAnalyticsService>();
            var recommendations = await advancedAnalytics.GenerateRecommendationsAsync(userId);

            _logger.LogInformation("Recommandations générées : {Count}", recommendations.Count);

            return Ok(new { recommendations });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération des recommandations");
            return StatusCode(500, new
            {
                error = "Impossible de générer les recommandations.",
                details = ex.Message
            });
        }
    }

    private int? GetUserIdFromToken()
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(claimValue, out var userId) ? userId : null;
    }
}

/// <summary>
/// DTO pour la requ�te de suggestion de cat�gorie
/// </summary>
public class CategorySuggestionRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

/// <summary>
/// DTO pour le chat IA
/// </summary>
public class ChatRequest
{
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Context { get; set; }
    public string? Page { get; set; }
}







