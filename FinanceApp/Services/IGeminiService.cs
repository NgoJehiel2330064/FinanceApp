namespace FinanceApp.Services;

/// <summary>
/// Interface pour le service d'intégration avec l'IA Gemini
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// Génère un conseil financier basé sur l'analyse de toutes les transactions
    /// </summary>
    /// <returns>Conseil financier court (15 mots maximum)</returns>
    /// <remarks>
    /// Cette méthode :
    /// 1. Récupère toutes les transactions depuis PostgreSQL
    /// 2. Analyse les données (revenus, dépenses, catégories)
    /// 3. Construit un prompt intelligent pour Gemini
    /// 4. Appelle l'API Gemini pour générer un conseil
    /// 5. Retourne le conseil en texte court
    /// </remarks>
    Task<string> GetFinancialAdvice();

    /// <summary>
    /// Analyse une transaction et suggère une catégorie avec l'IA
    /// </summary>
    /// <param name="description">Description de la transaction</param>
    /// <param name="amount">Montant de la transaction</param>
    /// <returns>Catégorie suggérée par l'IA</returns>
    Task<string> SuggestCategoryAsync(string description, decimal amount);

    /// <summary>
    /// Génère un résumé financier intelligent des transactions
    /// </summary>
    /// <param name="startDate">Date de début de la période</param>
    /// <param name="endDate">Date de fin de la période</param>
    /// <returns>Résumé généré par l'IA en langage naturel</returns>
    Task<string> GenerateFinancialSummaryAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Détecte les transactions inhabituelles (anomalies)
    /// </summary>
    /// <returns>Liste des transactions suspectes avec explication</returns>
    Task<List<string>> DetectAnomaliesAsync();

    /// <summary>
    /// Génère des prédictions budgétaires basées sur l'historique
    /// </summary>
    /// <param name="monthsAhead">Nombre de mois à prédire</param>
    /// <returns>Prédictions de revenus et dépenses</returns>
    Task<string> PredictBudgetAsync(int monthsAhead);
}
