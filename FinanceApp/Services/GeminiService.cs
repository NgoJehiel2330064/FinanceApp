using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FinanceApp.Services;

/// <summary>
/// Implémentation du service d'intégration avec l'IA Gemini
/// </summary>
/// <remarks>
/// Ce service utilise l'API REST de Google Gemini pour générer des conseils financiers
/// basés sur l'analyse des transactions de l'utilisateur
/// </remarks>
public class GeminiService : IGeminiService
{
    private readonly ILogger<GeminiService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    
    // URL de base de l'API Gemini
    private const string GEMINI_API_BASE_URL = "https://generativelanguage.googleapis.com/v1beta";

    /// <summary>
    /// Constructeur avec injection de dépendances
    /// </summary>
    /// <remarks>
    /// INJECTION DE DÉPENDANCES :
    /// - ILogger : Pour tracer les appels API et erreurs
    /// - IConfiguration : Pour lire la clé API depuis appsettings.json
    /// - ApplicationDbContext : Pour accéder aux transactions en base de données
    /// - HttpClient : Pour faire les appels HTTP vers l'API Gemini
    /// 
    /// HttpClient est injecté pour profiter du HttpClientFactory qui :
    /// - Gère le pool de connexions (réutilisation des sockets)
    /// - Évite l'épuisement des ports (socket exhaustion)
    /// - Gère automatiquement les DNS refresh
    /// </remarks>
    public GeminiService(
        ILogger<GeminiService> logger, 
        IConfiguration configuration,
        ApplicationDbContext context,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _context = context;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Génère un conseil financier basé sur l'analyse de toutes les transactions de l'utilisateur
    /// </summary>
    /// <param name="userId">ID de l'utilisateur dont analyser les transactions</param>
    /// <returns>Conseil financier court (15 mots maximum)</returns>
    /// <remarks>
    /// FLUX DE DONNÉES :
    /// 1. Récupère les transactions de l'utilisateur depuis PostgreSQL via EF Core
    /// 2. Analyse les données (calcul revenus, dépenses, catégories)
    /// 3. Construit un prompt pour Gemini avec un résumé
    /// 4. Appelle l'API Gemini via HTTP POST
    /// 5. Parse la réponse JSON
    /// 6. Retourne le conseil généré par l'IA
    /// 
    /// GESTION D'ERREURS :
    /// - Si pas de transactions : retourne un message par défaut
    /// - Si erreur API : log l'erreur et retourne un message de fallback
    /// - Si clé API manquante : retourne un message d'information
    /// </remarks>
    public async Task<string> GetFinancialAdvice(int userId)
    {
        try
        {
            _logger.LogInformation("Début de la génération du conseil financier pour userId={UserId}", userId);

            // ================================================================
            // ÉTAPE 1 : RÉCUPÉRATION DES TRANSACTIONS DEPUIS POSTGRESQL
            // ================================================================
            // ToListAsync() : Requête asynchrone qui génère le SQL :
            // SELECT * FROM "Transactions" WHERE "UserId" = {userId}
            // 
            // FLUX :
            // EF Core → Npgsql → TCP/IP → Docker → PostgreSQL Container
            // PostgreSQL exécute la requête et retourne les lignes
            // Docker → TCP/IP → Npgsql → EF Core → List<Transaction>
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (!transactions.Any())
            {
                _logger.LogInformation("Aucune transaction trouvée pour userId={UserId}", userId);
                return "Commencez à enregistrer vos transactions pour recevoir des conseils personnalisés.";
            }

            _logger.LogInformation("Récupération de {Count} transactions pour userId={UserId}", transactions.Count, userId);

            // ================================================================
            // ÉTAPE 2 : ANALYSE DES DONNÉES
            // ================================================================
            // Calcul des métriques financières pour créer un résumé intelligent
            
            // Where() + Sum() : Filtre et additionne les montants
            // C'est fait en mémoire (déjà chargé depuis la DB)
            var totalRevenue = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var totalExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            var balance = totalRevenue - totalExpenses;

            // GroupBy() : Regroupe par catégorie et calcule le total par catégorie
            // OrderByDescending() : Trie pour avoir la catégorie avec le plus de dépenses
            // FirstOrDefault() : Prend la première (celle avec le plus de dépenses)
            var topCategory = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();

            // ================================================================
            // ÉTAPE 3 : CONSTRUCTION DU PROMPT POUR GEMINI
            // ================================================================
            // Le prompt est crucial : il guide l'IA pour générer une réponse pertinente
            // On donne un contexte, des données, et des instructions précises
            var prompt = $@"Tu es un conseiller financier expert. Analyse ces données et donne UN conseil court (15 mots maximum).

Données financières :
- Revenus totaux : {totalRevenue:F2}€
- Dépenses totales : {totalExpenses:F2}€
- Balance : {balance:F2}€
- Catégorie avec le plus de dépenses : {topCategory?.Category ?? "N/A"} ({topCategory?.Total:F2}€)
- Nombre de transactions : {transactions.Count}

Conseil (15 mots max) :";

            _logger.LogInformation("Prompt créé pour userId={UserId}", userId);

            // ================================================================
            // ÉTAPE 4 : RÉCUPÉRATION DE LA CLÉ API
            // ================================================================
            // GetSection("Gemini") : Lit la section [Gemini] de appsettings.json
            // ["ApiKey"] : Accède à la propriété "ApiKey"
            var apiKey = _configuration.GetSection("Gemini")["ApiKey"];

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                _logger.LogWarning("Clé API Gemini non configurée");
                
                // En développement, on retourne un conseil simulé
                if (balance >= 0)
                    return "Votre solde est positif, continuez à bien gérer votre budget !";
                else
                    return "Attention, vos dépenses dépassent vos revenus. Analysez vos habitudes.";
            }

            // ================================================================
            // ÉTAPE 5 : APPEL À L'API GEMINI
            // ================================================================
            var advice = await CallGeminiApiAsync(prompt, apiKey);

            _logger.LogInformation("Conseil généré avec succès pour userId={UserId} : {Advice}", userId, advice);
            return advice;
        }
        catch (Exception ex)
        {
            // En cas d'erreur, on log et on retourne un message de fallback
            _logger.LogError(ex, "Erreur lors de la génération du conseil financier pour userId={UserId}", userId);
            return "Impossible de générer un conseil pour le moment. Vérifiez votre configuration.";
        }
    }

    /// <summary>
    /// Appelle l'API REST de Gemini pour générer du texte
    /// </summary>
    /// <param name="prompt">Le prompt à envoyer à l'IA</param>
    /// <param name="apiKey">Clé API Google</param>
    /// <returns>Réponse générée par Gemini</returns>
    /// <remarks>
    /// COMMUNICATION HTTP AVEC L'API GEMINI :
    /// 
    /// 1. Construction de l'URL avec la clé API
    /// 2. Création du payload JSON (corps de la requête)
    /// 3. Envoi de la requête POST
    /// 4. Réception de la réponse JSON
    /// 5. Parsing et extraction du texte généré
    /// 
    /// FORMAT DE L'API GEMINI :
    /// POST https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}
    /// 
    /// Body JSON :
    /// {
    ///   "contents": [{
    ///     "parts": [{
    ///       "text": "votre prompt"
    ///     }]
    ///   }],
    ///   "generationConfig": {
    ///     "temperature": 0.3,
    ///     "maxOutputTokens": 30
    ///   }
    /// }
    /// 
    /// Response JSON :
    /// {
    ///   "candidates": [{
    ///     "content": {
    ///       "parts": [{
    ///         "text": "réponse générée"
    ///       }]
    ///     }
    ///   }]
    /// }
    /// </remarks>
    private async Task<string> CallGeminiApiAsync(string prompt, string apiKey)
    {
        try
        {
            // Récupération de la configuration
            var model = _configuration.GetSection("Gemini")["Model"] ?? "gemini-1.5-flash";
            var temperature = float.Parse(_configuration.GetSection("Gemini")["Temperature"] ?? "0.3");
            var maxTokens = int.Parse(_configuration.GetSection("Gemini")["MaxTokens"] ?? "30");

            // Construction de l'URL de l'API
            var url = $"{GEMINI_API_BASE_URL}/models/{model}:generateContent?key={apiKey}";

            // Construction du payload JSON
            // JsonSerializer : Sérialise l'objet C# en JSON
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = temperature,
                    maxOutputTokens = maxTokens
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            _logger.LogDebug("Requête Gemini : {Json}", jsonContent);

            // Configuration du contenu HTTP
            // StringContent : Encapsule le JSON pour l'envoi HTTP
            // MediaTypeHeaderValue : Indique que c'est du JSON (Content-Type: application/json)
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // ================================================================
            // ENVOI DE LA REQUÊTE HTTP POST
            // ================================================================
            // PostAsync() : Envoi asynchrone de la requête
            // FLUX :
            // HttpClient → TCP/IP → Internet → Serveurs Google → API Gemini
            // API Gemini traite le prompt avec le modèle d'IA
            // Génère la réponse
            // Retour : API Gemini → Internet → TCP/IP → HttpClient
            var response = await _httpClient.PostAsync(url, content);

            // EnsureSuccessStatusCode() : Lève une exception si status ≠ 200-299
            response.EnsureSuccessStatusCode();

            // Lecture de la réponse JSON
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Réponse Gemini : {Response}", responseContent);

            // ================================================================
            // PARSING DE LA RÉPONSE JSON
            // ================================================================
            // JsonDocument : Parser JSON léger et performant
            // Parse() : Analyse le JSON en structure de données
            using var jsonDoc = JsonDocument.Parse(responseContent);
            
            // Navigation dans le JSON pour extraire le texte généré
            // jsonDoc.RootElement : Élément racine du JSON
            // GetProperty() : Accède à une propriété
            // [0] : Accède au premier élément du tableau
            var textGenerated = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            // Trim() : Supprime les espaces avant/après
            return textGenerated?.Trim() ?? "Conseil non disponible.";
        }
        catch (HttpRequestException ex)
        {
            // Erreur réseau ou erreur API (400, 401, 403, 500, etc.)
            _logger.LogError(ex, "Erreur HTTP lors de l'appel à l'API Gemini");
            throw new InvalidOperationException("Erreur de communication avec l'API Gemini", ex);
        }
        catch (JsonException ex)
        {
            // Erreur de parsing JSON (réponse invalide)
            _logger.LogError(ex, "Erreur de parsing de la réponse Gemini");
            throw new InvalidOperationException("Réponse invalide de l'API Gemini", ex);
        }
    }

    #region Autres méthodes de l'interface (implémentation complète)

    /// <inheritdoc />
    public async Task<string> SuggestCategoryAsync(int userId, string description, decimal amount)
    {
        try
        {
            _logger.LogInformation(
                "Suggestion de catégorie pour userId={UserId} : '{Description}', montant={Amount}€", 
                userId,
                description, 
                amount
            );

            // ================================================================
            // RÉCUPÉRATION DE LA CLÉ API
            // ================================================================
            var apiKey = _configuration.GetSection("Gemini")["ApiKey"];

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                _logger.LogWarning("Clé API Gemini non configurée");
                return "Non catégorisé";
            }

            // ================================================================
            // CONSTRUCTION DU PROMPT INTELLIGENT
            // ================================================================
            // On donne à Gemini une liste de catégories standard et le contexte
            var prompt = $@"Analyse cette transaction et suggère UNE catégorie parmi :
- Alimentation
- Transport
- Logement
- Loisirs
- Santé
- Éducation
- Vêtements
- Technologie
- Services
- Autres

Description : {description}
Montant : {amount:F2}€

Réponds UNIQUEMENT avec le nom exact de la catégorie, sans explication.";

            _logger.LogDebug("Prompt suggestion : {Prompt}", prompt);

            // ================================================================
            // APPEL À GEMINI AVEC PARAMÈTRES OPTIMISÉS
            // ================================================================
            // Pour une suggestion de catégorie, on veut :
            // - Très déterministe (temperature basse)
            // - Réponse ultra-courte (1 mot)
            var model = _configuration.GetSection("Gemini")["Model"] ?? "gemini-1.5-flash";
            var url = $"{GEMINI_API_BASE_URL}/models/{model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2f,      // Très déterministe
                    maxOutputTokens = 10     // 1 mot suffit
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseContent);
            
            var category = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()
                ?.Trim() ?? "Non catégorisé";

            _logger.LogInformation("Catégorie suggérée pour userId={UserId} : {Category}", userId, category);
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suggestion de catégorie pour userId={UserId}", userId);
            return "Non catégorisé";
        }
    }

    /// <inheritdoc />
    public async Task<string> GenerateFinancialSummaryAsync(int userId, DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInformation(
                "Génération du résumé financier pour userId={UserId}, période {StartDate} - {EndDate}", 
                userId,
                startDate, 
                endDate
            );

            // ================================================================
            // RÉCUPÉRATION DES TRANSACTIONS POUR LA PÉRIODE
            // ================================================================
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();

            if (!transactions.Any())
            {
                return $"Aucune transaction enregistrée entre le {startDate:dd/MM/yyyy} et le {endDate:dd/MM/yyyy}.";
            }

            // ================================================================
            // CALCUL DES MÉTRIQUES
            // ================================================================
            var totalRevenue = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var totalExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            var balance = totalRevenue - totalExpenses;

            // Top 3 catégories de dépenses
            var topCategories = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(3)
                .ToList();

            var categoriesText = string.Join(", ", topCategories.Select(c => $"{c.Category} ({c.Total:F2}€)"));

            // ================================================================
            // RÉCUPÉRATION DE LA CLÉ API
            // ================================================================
            var apiKey = _configuration.GetSection("Gemini")["ApiKey"];

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                _logger.LogWarning("Clé API Gemini non configurée");
                
                var tauxEpargne = totalRevenue > 0 ? ((balance / totalRevenue) * 100) : 0;
                return $"Période du {startDate:dd/MM/yyyy} au {endDate:dd/MM/yyyy} : " +
                       $"Revenus {totalRevenue:F2}€, Dépenses {totalExpenses:F2}€, " +
                       $"Solde {balance:F2}€ (taux d'épargne : {tauxEpargne:F1}%).";
            }

            // ================================================================
            // CONSTRUCTION DU PROMPT
            // ================================================================
            var prompt = $@"Génère un résumé financier concis et pédagogique pour la période du {startDate:dd/MM/yyyy} au {endDate:dd/MM/yyyy}.

Données financières :
- Revenus totaux : {totalRevenue:F2}€
- Dépenses totales : {totalExpenses:F2}€
- Balance : {balance:F2}€
- Nombre de transactions : {transactions.Count}
- Top 3 catégories de dépenses : {categoriesText}

Génère un résumé en 30-40 mots maximum, avec :
1. Une évaluation globale
2. Un point d'attention ou de félicitation
3. Une recommandation courte";

            _logger.LogDebug("Prompt résumé créé pour userId={UserId}", userId);

            // ================================================================
            // APPEL À GEMINI
            // ================================================================
            var model = _configuration.GetSection("Gemini")["Model"] ?? "gemini-1.5-flash";
            var url = $"{GEMINI_API_BASE_URL}/models/{model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.4f,      // Un peu créatif pour la rédaction
                    maxOutputTokens = 80     // Résumé plus long
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseContent);
            
            var summary = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()
                ?.Trim() ?? "Résumé non disponible.";

            _logger.LogInformation("Résumé généré pour userId={UserId} : {Summary}", userId, summary);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération du résumé financier pour userId={UserId}. Fallback activé.", userId);
            
            try
            {
                // Fallback : retourner un résumé basique sans Gemini
                var basicTransactions = await _context.Transactions
                    .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                    .ToListAsync();

                if (!basicTransactions.Any())
                {
                    return $"Aucune transaction enregistrée entre le {startDate:dd/MM/yyyy} et le {endDate:dd/MM/yyyy}.";
                }

                var basicRevenue = basicTransactions
                    .Where(t => t.Type == TransactionType.Income)
                    .Sum(t => t.Amount);

                var basicExpenses = basicTransactions
                    .Where(t => t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount);

                var basicBalance = basicRevenue - basicExpenses;
                var basicSavingsRate = basicRevenue > 0 ? ((basicBalance / basicRevenue) * 100) : 0;

                return $"Période {startDate:dd/MM} au {endDate:dd/MM} : " +
                       $"Revenus {basicRevenue:F2}€, Dépenses {basicExpenses:F2}€, " +
                       $"Épargne {basicBalance:F2}€ ({basicSavingsRate:F1}% du revenu).";
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Erreur même dans le fallback pour userId={UserId}", userId);
                return "Impossible de générer un résumé pour le moment. Vérifiez vos données de transactions.";
            }
        }
    }

    /// <inheritdoc />
    public async Task<List<string>> DetectAnomaliesAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Détection des anomalies pour userId={UserId}", userId);

            var anomalies = new List<string>();

            // ================================================================
            // RÉCUPÉRATION DES TRANSACTIONS
            // ================================================================
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (!transactions.Any())
            {
                return new List<string> { "Aucune transaction à analyser." };
            }

            // ================================================================
            // CALCUL DES MOYENNES PAR CATÉGORIE
            // ================================================================
            var expensesByCategory = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Average = g.Average(t => t.Amount),
                    Max = g.Max(t => t.Amount),
                    Transactions = g.ToList()
                })
                .ToList();

            // ================================================================
            // DÉTECTION D'ANOMALIES (ÉCART > 50% DE LA MOYENNE)
            // ================================================================
            foreach (var categoryGroup in expensesByCategory)
            {
                foreach (var transaction in categoryGroup.Transactions)
                {
                    // Si la transaction dépasse de 50% la moyenne, c'est une anomalie
                    if (transaction.Amount > categoryGroup.Average * 1.5m)
                    {
                        var ecartPercent = ((transaction.Amount - categoryGroup.Average) / categoryGroup.Average) * 100;
                        
                        anomalies.Add(
                            $"Dépense inhabituelle : {transaction.Amount:F2}€ en {transaction.Category} " +
                            $"le {transaction.Date:dd/MM/yyyy} (moyenne: {categoryGroup.Average:F2}€, écart: +{ecartPercent:F0}%)"
                        );
                    }
                }
            }

            // ================================================================
            // ENRICHISSEMENT AVEC GEMINI (OPTIONNEL)
            // ================================================================
            if (anomalies.Any())
            {
                var apiKey = _configuration.GetSection("Gemini")["ApiKey"];

                if (!string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_GEMINI_API_KEY_HERE")
                {
                    var prompt = $@"Voici des anomalies détectées dans les dépenses :

{string.Join("\n", anomalies)}

Pour chaque anomalie, génère une alerte courte et explicite (10 mots max par anomalie).
Format : une ligne par anomalie.";

                    try
                    {
                        var model = _configuration.GetSection("Gemini")["Model"] ?? "gemini-1.5-flash";
                        var url = $"{GEMINI_API_BASE_URL}/models/{model}:generateContent?key={apiKey}";

                        var requestBody = new
                        {
                            contents = new[]
                            {
                                new
                                {
                                    parts = new[]
                                    {
                                        new { text = prompt }
                                    }
                                }
                            },
                            generationConfig = new
                            {
                                temperature = 0.3f,
                                maxOutputTokens = 150
                            }
                        };

                        var jsonContent = JsonSerializer.Serialize(requestBody);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        var response = await _httpClient.PostAsync(url, content);
                        response.EnsureSuccessStatusCode();

                        var responseContent = await response.Content.ReadAsStringAsync();
                        using var jsonDoc = JsonDocument.Parse(responseContent);
                        
                        var enrichedAnomalies = jsonDoc.RootElement
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString()
                            ?.Trim()
                            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                            .ToList();

                        if (enrichedAnomalies != null && enrichedAnomalies.Any())
                        {
                            anomalies = enrichedAnomalies;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Impossible d'enrichir les anomalies avec Gemini pour userId={UserId}", userId);
                        // On garde les anomalies brutes
                    }
                }
            }

            if (!anomalies.Any())
            {
                anomalies.Add("Aucune anomalie détectée. Vos dépenses sont cohérentes !");
            }

            _logger.LogInformation("Détection terminée pour userId={UserId} : {Count} anomalie(s) trouvée(s)", userId, anomalies.Count);
            return anomalies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la détection d'anomalies pour userId={UserId}", userId);
            return new List<string> { "Impossible de détecter les anomalies pour le moment." };
        }
    }

    /// <inheritdoc />
    public async Task<string> PredictBudgetAsync(int userId, int monthsAhead)
    {
        try
        {
            _logger.LogInformation("Prédiction de budget pour userId={UserId}, {Months} prochains mois", userId, monthsAhead);

            if (monthsAhead <= 0 || monthsAhead > 12)
            {
                return "Le nombre de mois doit être entre 1 et 12.";
            }

            // ================================================================
            // RÉCUPÉRATION DES TRANSACTIONS DES 3 DERNIERS MOIS
            // ================================================================
            var threeMonthsAgo = DateTime.Now.AddMonths(-3);
            var recentTransactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= threeMonthsAgo)
                .ToListAsync();

            if (!recentTransactions.Any())
            {
                return "Pas assez de données historiques pour faire une prédiction (minimum 3 mois).";
            }

            // ================================================================
            // CALCUL DES MOYENNES MENSUELLES
            // ================================================================
            var monthlyRevenue = recentTransactions
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => g.Sum(t => t.Amount))
                .Average();

            var monthlyExpenses = recentTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => g.Sum(t => t.Amount))
                .Average();

            var monthlySavings = monthlyRevenue - monthlyExpenses;

            // Calcul de la tendance (simple : comparaison premier/dernier mois)
            var firstMonthExpenses = recentTransactions
                .Where(t => t.Type == TransactionType.Expense && t.Date < DateTime.Now.AddMonths(-2))
                .Sum(t => t.Amount);

            var lastMonthExpenses = recentTransactions
                .Where(t => t.Type == TransactionType.Expense && t.Date >= DateTime.Now.AddMonths(-1))
                .Sum(t => t.Amount);

            var trend = lastMonthExpenses > firstMonthExpenses ? "hausse" : "baisse";
            var trendPercent = firstMonthExpenses > 0 
                ? Math.Abs(((lastMonthExpenses - firstMonthExpenses) / firstMonthExpenses) * 100)
                : 0;

            // ================================================================
            // RÉCUPÉRATION DE LA CLÉ API
            // ================================================================
            var apiKey = _configuration.GetSection("Gemini")["ApiKey"];

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                _logger.LogWarning("Clé API Gemini non configurée");
                
                var predictedSavings = monthlySavings * monthsAhead;
                return $"Prédiction simple : Avec vos habitudes actuelles, vous économiserez environ {predictedSavings:F2}€ " +
                       $"dans {monthsAhead} mois (économies mensuelles moyennes : {monthlySavings:F2}€).";
            }

            // ================================================================
            // CONSTRUCTION DU PROMPT
            // ================================================================
            var prompt = $@"Prédis le budget pour les {monthsAhead} prochains mois en te basant sur ces données.

Historique des 3 derniers mois :
- Revenus moyens : {monthlyRevenue:F2}€/mois
- Dépenses moyennes : {monthlyExpenses:F2}€/mois
- Économies mensuelles : {monthlySavings:F2}€
- Tendance des dépenses : {trend} de {trendPercent:F1}%

Génère une prédiction réaliste et motivante en 25 mots maximum.
Inclus une estimation chiffrée des économies potentielles.";

            _logger.LogDebug("Prompt prédiction créé pour userId={UserId}", userId);

            // ================================================================
            // APPEL À GEMINI
            // ================================================================
            var model = _configuration.GetSection("Gemini")["Model"] ?? "gemini-1.5-flash";
            var url = $"{GEMINI_API_BASE_URL}/models/{model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.5f,      // Plus créatif pour les prédictions
                    maxOutputTokens = 50
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseContent);
            
            var prediction = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()
                ?.Trim() ?? "Prédiction non disponible.";

            _logger.LogInformation("Prédiction générée pour userId={UserId} : {Prediction}", userId, prediction);
            return prediction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la prédiction de budget pour userId={UserId}", userId);
            return "Impossible de générer une prédiction pour le moment.";
        }
    }

    /// <inheritdoc />
    public async Task<List<string>> GetPortfolioInsightsAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Génération d'insights patrimoniaux pour userId={UserId}", userId);

            // ================================================================
            // RÉCUPÉRATION DES DONNÉES (Assets + Transactions)
            // ================================================================
            var assets = await _context.Assets
                .Where(a => a.UserId == userId)
                .ToListAsync();
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (!assets.Any())
            {
                return new List<string> 
                { 
                    "Aucun asset enregistré. Commencez par ajouter vos biens pour une analyse patrimoniale complète." 
                };
            }

            // ================================================================
            // CALCUL DES MÉTRIQUES PATRIMONIALES
            // ================================================================
            
            // 1. Valeur totale du patrimoine
            var totalAssetValue = assets.Sum(a => a.CurrentValue);

            // 2. Répartition par type d'asset
            var assetsByType = assets
                .GroupBy(a => a.Type)
                .Select(g => new
                {
                    Type = g.Key.ToString(),
                    TotalValue = g.Sum(a => a.CurrentValue),
                    Percentage = (g.Sum(a => a.CurrentValue) / totalAssetValue) * 100,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.TotalValue)
                .ToList();

            // 3. Calcul des flux financiers (3 derniers mois)
            var threeMonthsAgo = DateTime.Now.AddMonths(-3);
            var recentTransactions = transactions
                .Where(t => t.Date >= threeMonthsAgo)
                .ToList();

            decimal avgMonthlyRevenue = 0;
            decimal avgMonthlyExpenses = 0;
            decimal avgMonthlySavings = 0;

            if (recentTransactions.Any())
            {
                var monthlyRevenues = recentTransactions
                    .Where(t => t.Type == TransactionType.Income)
                    .GroupBy(t => new { t.Date.Year, t.Date.Month })
                    .Select(g => g.Sum(t => t.Amount));

                var monthlyExpenses = recentTransactions
                    .Where(t => t.Type == TransactionType.Expense)
                    .GroupBy(t => new { t.Date.Year, t.Date.Month })
                    .Select(g => g.Sum(t => t.Amount));

                avgMonthlyRevenue = monthlyRevenues.Any() ? monthlyRevenues.Average() : 0;
                avgMonthlyExpenses = monthlyExpenses.Any() ? monthlyExpenses.Average() : 0;
                avgMonthlySavings = avgMonthlyRevenue - avgMonthlyExpenses;
            }

            // 4. Calcul des ratios clés
            var revenueToAssetRatio = totalAssetValue > 0 
                ? (avgMonthlyRevenue / totalAssetValue) * 100 
                : 0;

            // 5. Heuristique des assets productifs
            // Investment = productif, autres = moins productifs
            var productiveAssets = assets.Where(a => a.Type == AssetType.Investment).Sum(a => a.CurrentValue);
            var productiveRatio = totalAssetValue > 0 
                ? (productiveAssets / totalAssetValue) * 100 
                : 0;

            // 6. Taux d'épargne
            var savingsRate = avgMonthlyRevenue > 0 
                ? (avgMonthlySavings / avgMonthlyRevenue) * 100 
                : 0;

            // ================================================================
            // CONSTRUCTION DU PROMPT STRUCTURÉ POUR GEMINI
            // ================================================================
            var assetBreakdown = string.Join("\n", assetsByType.Select(a => 
                $"  - {a.Type} : {a.Percentage:F1}% ({a.TotalValue:F2} CAD, {a.Count} actif(s))"
            ));

            var prompt = $@"Tu es un conseiller patrimonial expert. Analyse le patrimoine suivant et fournis EXACTEMENT 3 insights stratégiques courts.

DONNÉES PATRIMONIALES :
???????????????????????
Valeur Totale : {totalAssetValue:F2} CAD
Nombre d'actifs : {assets.Count}

Répartition par Type :
{assetBreakdown}

DONNÉES DE FLUX (3 derniers mois) :
???????????????????????
Revenus mensuels moyens : {avgMonthlyRevenue:F2} CAD
Dépenses mensuelles moyennes : {avgMonthlyExpenses:F2} CAD
Épargne mensuelle moyenne : {avgMonthlySavings:F2} CAD

RATIOS CLÉS :
???????????????????????
Ratio revenus / patrimoine : {revenueToAssetRatio:F2}% par mois
Assets productifs (Investissements) : {productiveRatio:F1}% du total
Taux d'épargne : {savingsRate:F1}%

RÈGLES DE GÉNÉRATION :
???????????????????????
1. Génère EXACTEMENT 3 insights (pas plus, pas moins)
2. Chaque insight : 15-20 mots maximum
3. Ton professionnel et factuel
4. Pas de recommandations d'investissement risquées
5. Focus sur la structure du patrimoine et les flux
6. Utilise des chiffres concrets

FORMAT DE RÉPONSE (STRICT) :
???????????????????????
Retourne UNIQUEMENT les 3 insights sous forme de liste numérotée :
1. [Premier insight sur la structure du patrimoine]
2. [Deuxième insight sur les flux et ratios]
3. [Troisième insight stratégique ou recommandation]";

            _logger.LogDebug("Prompt portfolio insights créé pour userId={UserId}", userId);

            // ================================================================
            // RÉCUPÉRATION DE LA CLÉ API
            // ================================================================
            var apiKey = _configuration.GetSection("Gemini")["ApiKey"];

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
            {
                _logger.LogWarning("Clé API Gemini non configurée - Génération d'insights basiques pour userId={UserId}", userId);
                
                // Fallback sans IA : insights basiques
                var fallbackInsights = new List<string>();

                // Insight 1 : Concentration
                var dominantType = assetsByType.FirstOrDefault();
                if (dominantType != null && dominantType.Percentage > 60)
                {
                    fallbackInsights.Add(
                        $"Votre patrimoine est fortement concentré en {dominantType.Type} ({dominantType.Percentage:F0}%), ce qui limite la diversification."
                    );
                }
                else
                {
                    fallbackInsights.Add("Votre patrimoine est diversifié entre plusieurs types d'actifs.");
                }

                // Insight 2 : Ratio revenus/patrimoine
                if (revenueToAssetRatio < 0.5m)
                {
                    fallbackInsights.Add(
                        $"Vos revenus mensuels ({avgMonthlyRevenue:F0} CAD) sont faibles par rapport à votre patrimoine ({totalAssetValue:F0} CAD)."
                    );
                }
                else
                {
                    fallbackInsights.Add(
                        $"Vos revenus mensuels ({avgMonthlyRevenue:F0} CAD) sont proportionnés à votre patrimoine."
                    );
                }

                // Insight 3 : Assets productifs
                if (productiveRatio < 20)
                {
                    fallbackInsights.Add(
                        $"Seulement {productiveRatio:F0}% de vos actifs sont des investissements productifs."
                    );
                }
                else
                {
                    fallbackInsights.Add(
                        $"Environ {productiveRatio:F0}% de votre patrimoine est investi dans des actifs productifs."
                    );
                }

                return fallbackInsights;
            }

            // ================================================================
            // APPEL À GEMINI
            // ================================================================
            var model = _configuration.GetSection("Gemini")["Model"] ?? "gemini-1.5-flash";
            var url = $"{GEMINI_API_BASE_URL}/models/{model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.4f,      // Équilibré entre créativité et cohérence
                    maxOutputTokens = 120    // 3 insights x ~40 tokens
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseContent);
            
            var generatedText = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()
                ?.Trim();

            // ================================================================
            // PARSING DE LA RÉPONSE (EXTRACTION DES 3 INSIGHTS)
            // ================================================================
            var insights = new List<string>();

            if (!string.IsNullOrEmpty(generatedText))
            {
                // Parse les lignes numérotées (1. 2. 3.)
                var lines = generatedText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    
                    // Accepter les formats : "1. texte", "1) texte", "- texte"
                    if (System.Text.RegularExpressions.Regex.IsMatch(trimmedLine, @"^[0-9]+[\.\)]\s+.*") ||
                        trimmedLine.StartsWith("- "))
                    {
                        // Nettoyer le numéro/marqueur au début
                        var cleanedInsight = System.Text.RegularExpressions.Regex.Replace(
                            trimmedLine, 
                            @"^[0-9]+[\.\)]\s+|-\s+", 
                            ""
                        ).Trim();

                        if (!string.IsNullOrWhiteSpace(cleanedInsight))
                        {
                            insights.Add(cleanedInsight);
                        }
                    }
                }

                // Si moins de 3 insights parsés, ajouter le texte brut
                if (insights.Count < 3)
                {
                    insights.Clear();
                    insights.Add(generatedText);
                }

                // Limiter à 3 insights maximum
                insights = insights.Take(3).ToList();
            }

            if (!insights.Any())
            {
                insights.Add("Impossible de générer des insights pour le moment.");
            }

            _logger.LogInformation("Insights générés pour userId={UserId} : {Count} insight(s)", userId, insights.Count);
            return insights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération d'insights patrimoniaux pour userId={UserId}", userId);
            return new List<string> 
            { 
                "Impossible de générer des insights pour le moment. Vérifiez votre configuration." 
            };
        }
    }

    #endregion

    public Task<string> GetChatResponseAsync(int userId, string message, string? context)
    {
        // Service Gemini conservé pour compatibilité. En prod, GroqAIService est utilisé.
        return Task.FromResult("Chat IA indisponible sur ce service. Veuillez utiliser Groq.");
    }
}
