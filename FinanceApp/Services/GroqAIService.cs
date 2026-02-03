using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace FinanceApp.Services;

/// <summary>
/// Service Groq pour l'IA financière (ultra-rapide, gratuit)
/// </summary>
public class GroqAIService : IGeminiService
{
    private readonly ILogger<GroqAIService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;

    public GroqAIService(
        ILogger<GroqAIService> logger,
        IConfiguration configuration,
        ApplicationDbContext context,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _context = context;
        _httpClient = httpClient;
    }

    public async Task<string> GetFinancialAdvice(int userId)
    {
        try
        {
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (!transactions.Any())
            {
                return "Commencez à enregistrer vos transactions pour recevoir des conseils personnalisés.";
            }

            var totalRevenue = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var totalExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            var balance = totalRevenue - totalExpenses;

            var topCategory = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();

            var prompt = $@"Tu es un conseiller financier expert. Analyse ces données et donne UN conseil court (15 mots maximum).

Données financières :
- Revenus totaux : {totalRevenue:F2}€
- Dépenses totales : {totalExpenses:F2}€
- Balance : {balance:F2}€
- Catégorie avec le plus de dépenses : {topCategory?.Category ?? "N/A"} ({topCategory?.Total:F2}€)
- Nombre de transactions : {transactions.Count}

Conseil (15 mots max) :";

            return await CallGroqAsync(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération du conseil financier");
            return "Impossible de générer un conseil pour le moment.";
        }
    }

    public async Task<string> GenerateFinancialSummaryAsync(int userId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();

            if (!transactions.Any())
            {
                return $"Aucune transaction enregistrée entre le {startDate:dd/MM/yyyy} et le {endDate:dd/MM/yyyy}.";
            }

            var totalRevenue = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var totalExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            var balance = totalRevenue - totalExpenses;

            var topCategories = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(3)
                .ToList();

            var categoriesText = string.Join(", ", topCategories.Select(c => $"{c.Category} ({c.Total:F2}€)"));

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

            return await CallGroqAsync(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération du résumé financier");
            return GetBasicSummary(userId, startDate, endDate);
        }
    }

    public async Task<string> GetChatResponseAsync(int userId, string message, string? context)
    {
        try
        {
            var safeContext = string.IsNullOrWhiteSpace(context)
                ? "Contexte indisponible."
                : context;

            var prompt = $@"Tu es un assistant financier conversationnel. Tu as accès aux données suivantes :
{safeContext}

Règles :
- Réponds en français de manière concise
- Sois précis et actionnable
- Si l'utilisateur demande des informations sur ses transactions, redirige-le vers /transactions
- Si l'utilisateur demande des statistiques ou graphiques, redirige-le vers /statistiques  
- Si l'utilisateur demande des informations sur son patrimoine ou actifs, redirige-le vers /patrimoine
- Si l'utilisateur veut modifier son profil, redirige-le vers /profil
- Pour une redirection, indique clairement la page avec son chemin entre crochets

Question utilisateur : {message}

Réponse :";

            return await CallGroqAsync(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération de la réponse chat Groq");
            return "Je n'arrive pas à répondre pour le moment. Pouvez-vous reformuler ?";
        }
    }

    private async Task<string> CallGroqAsync(string prompt)
    {
        try
        {
            var apiKey = _configuration.GetSection("Groq")["ApiKey"];
            var model = _configuration.GetSection("Groq")["Model"] ?? "mixtral-8x7b-32768";
            var baseUrl = _configuration.GetSection("Groq")["BaseUrl"] ?? "https://api.groq.com/openai/v1";

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Clé API Groq non configurée");
                return "Clé API non configurée.";
            }

            var url = $"{baseUrl}/chat/completions";

            var temperatureValue = _configuration.GetSection("Groq")["Temperature"];
            var maxTokensValue = _configuration.GetSection("Groq")["MaxTokens"];
            var temperature = 0.3;
            var maxTokens = 150;

            if (double.TryParse(temperatureValue, out var parsedTemp))
            {
                temperature = parsedTemp;
            }

            if (int.TryParse(maxTokensValue, out var parsedTokens))
            {
                maxTokens = parsedTokens;
            }

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = temperature,
                max_tokens = maxTokens
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseContent);

            var message = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?.Trim() ?? "Réponse non disponible.";

            _logger.LogInformation("Réponse Groq générée : {Message}", message);
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'appel à Groq");
            throw;
        }
    }

    private string GetBasicSummary(int userId, DateTime startDate, DateTime endDate)
    {
        return $"Analyse pour la période du {startDate:dd/MM} au {endDate:dd/MM}. " +
               $"Consultez votre historique de transactions pour plus de détails.";
    }

    public Task<string> SuggestCategoryAsync(int userId, string description, decimal amount) => Task.FromResult("Other");
    public Task<List<string>> DetectAnomaliesAsync(int userId) => Task.FromResult(new List<string>());
    public Task<string> PredictBudgetAsync(int userId, int monthsAhead) => Task.FromResult("Prédictions non disponibles.");
    public Task<List<string>> GetPortfolioInsightsAsync(int userId) => Task.FromResult(new List<string>());
}
