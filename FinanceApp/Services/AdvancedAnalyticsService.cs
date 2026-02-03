using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services;

/// <summary>
/// Service d'analyse avancée des finances pour détecter patterns, anomalies et recommandations
/// </summary>
public class AdvancedAnalyticsService : IAdvancedAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdvancedAnalyticsService> _logger;

    public AdvancedAnalyticsService(
        ApplicationDbContext context,
        ILogger<AdvancedAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Analyse les patterns de dépenses de l'utilisateur pour la dernière période
    /// </summary>
    public async Task<SpendingPatterns> AnalyzeSpendingPatternsAsync(int userId, int monthsToAnalyze = 3)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddMonths(-monthsToAnalyze);
            
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startDate && t.Type == TransactionType.Expense)
                .ToListAsync();

            if (!transactions.Any())
            {
                return new SpendingPatterns
                {
                    TotalTransactions = 0,
                    Categories = new Dictionary<string, CategoryPattern>()
                };
            }

            var totalExpenses = transactions.Sum(t => t.Amount);
            var categoryData = transactions
                .GroupBy(t => t.Category)
                .ToDictionary(
                    g => g.Key,
                    g => new CategoryPattern
                    {
                        Category = g.Key ?? "Other",
                        TotalSpent = g.Sum(t => t.Amount),
                        TransactionCount = g.Count(),
                        AverageTransaction = Math.Round(g.Average(t => t.Amount), 2),
                        Percentage = totalExpenses > 0 ? Math.Round(((double)(g.Sum(t => t.Amount)) / (double)totalExpenses) * 100, 2) : 0,
                        LastTransaction = g.OrderByDescending(t => t.Date).First().Date,
                        IsRecurring = DetectRecurringPattern(g.ToList())
                    }
                );

            var monthlyTotals = transactions
                .GroupBy(t => t.Date.Year * 100 + t.Date.Month)
                .OrderByDescending(g => g.Key)
                .Take(3)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            return new SpendingPatterns
            {
                TotalTransactions = transactions.Count,
                TotalSpent = totalExpenses,
                AverageMonthlySpending = monthlyTotals.Count > 0 ? monthlyTotals.Values.Average() : 0,
                HighestSpendingMonth = monthlyTotals.Count > 0 ? monthlyTotals.Max(x => x.Value) : 0,
                LowestSpendingMonth = monthlyTotals.Count > 0 ? monthlyTotals.Min(x => x.Value) : 0,
                Categories = categoryData.OrderByDescending(x => x.Value.TotalSpent).ToDictionary(x => x.Key, x => x.Value),
                AnalysisPeriod = monthsToAnalyze,
                MostSpentCategory = categoryData.OrderByDescending(x => x.Value.TotalSpent).FirstOrDefault().Key ?? "Other",
                SpendingVariance = CalculateSpendingVariance(monthlyTotals.Values.ToList()),
                TrendDirection = CalculateTrend(monthlyTotals.Values.ToList())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse des patterns de dépenses pour userId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Détecte les anomalies dans les dépenses (montants inhabituels, dépenses rares)
    /// </summary>
    public async Task<AnomalyReport> DetectAnomaliesAsync(int userId)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddMonths(-3);
            var allTransactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startDate && t.Type == TransactionType.Expense)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            var anomalies = new List<Anomaly>();

            // Grouper par catégorie pour l'analyse
            var transactionsByCategory = allTransactions.GroupBy(t => t.Category);

            foreach (var categoryGroup in transactionsByCategory)
            {
                var categoryTransactions = categoryGroup.ToList();
                var amounts = categoryTransactions.Select(t => t.Amount).ToList();
                
                // Calculer statistiques
                var average = amounts.Average();
                var stdDev = CalculateStandardDeviation(amounts);

                // Détecter les montants anormalement élevés (> moyenne + 2*écart-type)
                foreach (var transaction in categoryTransactions.OrderByDescending(t => t.Date).Take(10))
                {
                    if (transaction.Amount > average + (decimal)stdDev * 2)
                    {
                        var percentageDifference = Math.Round(((transaction.Amount - average) / average) * 100, 2);
                        
                        anomalies.Add(new Anomaly
                        {
                            TransactionId = transaction.Id,
                            Description = transaction.Description ?? "",
                            Category = transaction.Category ?? "Other",
                            Amount = transaction.Amount,
                            Date = transaction.Date,
                            AnomalyType = "UnusualAmount",
                            Severity = percentageDifference > 100 ? "High" : "Medium",
                            Message = $"Dépense anormalement élevée en {transaction.Category}: {transaction.Amount:C} ({percentageDifference}% au-dessus de la moyenne)",
                            ExpectedRange = new { Min = average - (decimal)stdDev, Max = average + (decimal)stdDev }
                        });
                    }
                }
            }

            // Détecter les catégories rarement utilisées avec dépense récente
            var categoryFrequency = allTransactions.GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Count = g.Count(), LastTransaction = g.Max(t => t.Date) })
                .Where(x => x.Count <= 2 && (DateTime.UtcNow - x.LastTransaction).TotalDays < 7)
                .ToList();

            foreach (var category in categoryFrequency)
            {
                var recentTransaction = allTransactions
                    .Where(t => t.Category == category.Category)
                    .OrderByDescending(t => t.Date)
                    .First();

                anomalies.Add(new Anomaly
                {
                    TransactionId = recentTransaction.Id,
                    Description = recentTransaction.Description ?? "",
                    Category = recentTransaction.Category ?? "Other",
                    Amount = recentTransaction.Amount,
                    Date = recentTransaction.Date,
                    AnomalyType = "UnusualCategory",
                    Severity = "Low",
                    Message = $"Catégorie inhabituelle: {recentTransaction.Category} (peu fréquente)"
                });
            }

            return new AnomalyReport
            {
                UserId = userId,
                AnalysisDate = DateTime.UtcNow,
                TotalAnomalies = anomalies.Count,
                HighSeverityCount = anomalies.Count(a => a.Severity == "High"),
                MediumSeverityCount = anomalies.Count(a => a.Severity == "Medium"),
                LowSeverityCount = anomalies.Count(a => a.Severity == "Low"),
                Anomalies = anomalies.OrderByDescending(a => a.Amount).Take(10).ToList(),
                HasCriticalAnomalies = anomalies.Any(a => a.Severity == "High" && a.Amount > 500)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la détection d'anomalies pour userId={UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Génère des recommandations personnalisées basées sur les patterns de dépenses
    /// </summary>
    public async Task<List<PersonalizedRecommendation>> GenerateRecommendationsAsync(int userId)
    {
        try
        {
            var patterns = await AnalyzeSpendingPatternsAsync(userId);
            var anomalies = await DetectAnomaliesAsync(userId);
            var recommendations = new List<PersonalizedRecommendation>();

            // Recommandation 1: Catégorie avec plus de dépenses
            if (patterns.Categories.Count > 0)
            {
                var topCategory = patterns.Categories.Values.OrderByDescending(c => c.Percentage).First();
                if (topCategory.Percentage > 40)
                {
                    recommendations.Add(new PersonalizedRecommendation
                    {
                        Type = "ReduceSpending",
                        Category = topCategory.Category,
                        Title = $"Réduire les dépenses en {topCategory.Category}",
                        Description = $"Votre catégorie '{topCategory.Category}' représente {topCategory.Percentage}% de vos dépenses (moyenne nationale: 15-20%). " +
                                    $"Réduire de seulement 10% vous permettrait d'économiser environ {Math.Round(topCategory.TotalSpent * 0.1m, 2):C} par mois.",
                        PotentialSavings = Math.Round(topCategory.TotalSpent * 0.1m, 2),
                        Priority = "High",
                        Icon = "📉"
                    });
                }
            }

            // Recommandation 2: Transactions anormales
            if (anomalies.HighSeverityCount > 0)
            {
                var highAnomalies = anomalies.Anomalies.Where(a => a.Severity == "High").ToList();
                if (highAnomalies.Count > 0)
                {
                    var totalHighAnomalies = highAnomalies.Sum(a => a.Amount);
                    recommendations.Add(new PersonalizedRecommendation
                    {
                        Type = "ReviewAnomalies",
                        Title = "Vérifier les dépenses inhabituelles",
                        Description = $"Vous avez {highAnomalies.Count} dépense(s) anormalement élevée(s) totalisant {totalHighAnomalies:C}. " +
                                    "Vérifiez si ces montants sont justifiés ou s'il s'agit d'erreurs.",
                        PotentialSavings = totalHighAnomalies,
                        Priority = "High",
                        Icon = "⚠️"
                    });
                }
            }

            // Recommandation 3: Catégories avec dépenses récurrentes
            var recurringCategories = patterns.Categories.Values.Where(c => c.IsRecurring && c.Percentage > 5).ToList();
            if (recurringCategories.Any())
            {
                var mostRecurring = recurringCategories.OrderByDescending(c => c.Percentage).First();
                recommendations.Add(new PersonalizedRecommendation
                {
                    Type = "OptimizeRecurring",
                    Category = mostRecurring.Category,
                    Title = $"Optimiser vos dépenses récurrentes en {mostRecurring.Category}",
                    Description = $"Vous dépensez régulièrement en {mostRecurring.Category}. " +
                                "Cherchez des alternatives moins chères ou des abonnements à résilier.",
                    PotentialSavings = Math.Round(mostRecurring.TotalSpent * 0.15m, 2),
                    Priority = "Medium",
                    Icon = "🔄"
                });
            }

            // Recommandation 4: Budget quotidien
            if (patterns.AverageMonthlySpending > 0)
            {
                var recommendedDailyBudget = Math.Round(patterns.AverageMonthlySpending / 30 * 0.9m, 2);
                recommendations.Add(new PersonalizedRecommendation
                {
                    Type = "DailyBudget",
                    Title = "Établir un budget quotidien",
                    Description = $"Votre moyenne mensuelle est de {patterns.AverageMonthlySpending:C}. " +
                                $"Essayez un budget quotidien de {recommendedDailyBudget:C} pour économiser 10%.",
                    PotentialSavings = Math.Round(patterns.AverageMonthlySpending * 0.1m, 2),
                    Priority = "Medium",
                    Icon = "💰"
                });
            }

            // Recommandation 5: Volatilité des dépenses
            if (patterns.SpendingVariance > 30)
            {
                recommendations.Add(new PersonalizedRecommendation
                {
                    Type = "StabilizeSpending",
                    Title = "Stabiliser vos dépenses",
                    Description = $"Vos dépenses varient fortement d'un mois à l'autre ({patterns.SpendingVariance}% de variation). " +
                                "Essayez de lisser vos dépenses pour mieux planifier votre budget.",
                    Priority = "Low",
                    Icon = "📊"
                });
            }

            return recommendations.OrderByDescending(r => r.Priority == "High" ? 3 : r.Priority == "Medium" ? 2 : 1).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération des recommandations pour userId={UserId}", userId);
            throw;
        }
    }

    // ==================== HELPERS ====================

    private bool DetectRecurringPattern(List<Transaction> transactions)
    {
        if (transactions.Count < 2) return false;

        var sorted = transactions.OrderByDescending(t => t.Date).ToList();
        var lastTransaction = sorted.First();
        var secondLastTransaction = sorted.ElementAtOrDefault(1);

        if (secondLastTransaction == null) return false;

        // Vérifier si montants similaires et dates espacées régulièrement
        var amountDifference = (double)(Math.Abs(lastTransaction.Amount - secondLastTransaction.Amount) / lastTransaction.Amount);
        var daysDifference = (lastTransaction.Date - secondLastTransaction.Date).TotalDays;

        return amountDifference < 0.1 && daysDifference > 20 && daysDifference < 40;
    }

    private double CalculateStandardDeviation(List<decimal> values)
    {
        if (values.Count < 2) return 0;

        var average = values.Average();
        var sumOfSquaresDifferences = values.Sum(v => Math.Pow((double)(v - average), 2));
        return Math.Sqrt(sumOfSquaresDifferences / (values.Count - 1));
    }

    private double CalculateSpendingVariance(List<decimal> monthlyTotals)
    {
        if (monthlyTotals.Count < 2) return 0;

        var average = monthlyTotals.Average();
        var variance = monthlyTotals.Sum(t => Math.Pow((double)(t - average), 2)) / monthlyTotals.Count;
        var stdDev = Math.Sqrt(variance);
        
        return average > 0 ? Math.Round((stdDev / (double)average) * 100, 2) : 0;
    }

    private string CalculateTrend(List<decimal> monthlyTotals)
    {
        if (monthlyTotals.Count < 2) return "Neutral";

        var recentMonths = monthlyTotals.OrderByDescending(x => x).Take(2).ToList();
        if (recentMonths.Count < 2) return "Neutral";

        var percentageChange = ((recentMonths[1] - recentMonths[0]) / recentMonths[0]) * 100;

        if (percentageChange > 10) return "Increasing";
        if (percentageChange < -10) return "Decreasing";
        return "Neutral";
    }
}

// ==================== MODÈLES DE DONNÉES ====================

public interface IAdvancedAnalyticsService
{
    Task<SpendingPatterns> AnalyzeSpendingPatternsAsync(int userId, int monthsToAnalyze = 3);
    Task<AnomalyReport> DetectAnomaliesAsync(int userId);
    Task<List<PersonalizedRecommendation>> GenerateRecommendationsAsync(int userId);
}

public class SpendingPatterns
{
    public int TotalTransactions { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageMonthlySpending { get; set; }
    public decimal HighestSpendingMonth { get; set; }
    public decimal LowestSpendingMonth { get; set; }
    public double SpendingVariance { get; set; }
    public string? TrendDirection { get; set; }
    public int AnalysisPeriod { get; set; }
    public string? MostSpentCategory { get; set; }
    public Dictionary<string, CategoryPattern> Categories { get; set; } = new();
}

public class CategoryPattern
{
    public string? Category { get; set; }
    public decimal TotalSpent { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageTransaction { get; set; }
    public double Percentage { get; set; }
    public DateTime LastTransaction { get; set; }
    public bool IsRecurring { get; set; }
}

public class AnomalyReport
{
    public int UserId { get; set; }
    public DateTime AnalysisDate { get; set; }
    public int TotalAnomalies { get; set; }
    public int HighSeverityCount { get; set; }
    public int MediumSeverityCount { get; set; }
    public int LowSeverityCount { get; set; }
    public bool HasCriticalAnomalies { get; set; }
    public List<Anomaly> Anomalies { get; set; } = new();
}

public class Anomaly
{
    public int TransactionId { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? AnomalyType { get; set; }
    public string? Severity { get; set; }
    public string? Message { get; set; }
    public object? ExpectedRange { get; set; }
}

public class PersonalizedRecommendation
{
    public string? Type { get; set; }
    public string? Category { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal PotentialSavings { get; set; }
    public string? Priority { get; set; }
    public string? Icon { get; set; }
}
