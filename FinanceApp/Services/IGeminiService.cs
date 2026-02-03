namespace FinanceApp.Services;

/// <summary>
/// Interface pour le service d'int�gration avec l'IA Gemini
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// G�n�re un conseil financier bas� sur l'analyse de toutes les transactions
    /// </summary>
    /// <returns>Conseil financier court (15 mots maximum)</returns>
    /// <remarks>
    /// Cette m�thode :
    /// 1. R�cup�re toutes les transactions depuis PostgreSQL
    /// 2. Analyse les donn�es (revenus, d�penses, cat�gories)
    /// 3. Construit un prompt intelligent pour Gemini
    /// 4. Appelle l'API Gemini pour g�n�rer un conseil
    /// 5. Retourne le conseil en texte court
    /// </remarks>
    Task<string> GetFinancialAdvice(int userId);

    /// <summary>
    /// Analyse une transaction et sugg�re une cat�gorie avec l'IA
    /// </summary>
    /// <param name="description">Description de la transaction</param>
    /// <param name="amount">Montant de la transaction</param>
    /// <returns>Cat�gorie sugg�r�e par l'IA</returns>
    Task<string> SuggestCategoryAsync(int userId, string description, decimal amount);

    /// <summary>
    /// G�n�re un r�sum� financier intelligent des transactions
    /// </summary>
    /// <param name="startDate">Date de d�but de la p�riode</param>
    /// <param name="endDate">Date de fin de la p�riode</param>
    /// <returns>R�sum� g�n�r� par l'IA en langage naturel</returns>
    Task<string> GenerateFinancialSummaryAsync(int userId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Chat conversationnel avec l'IA, contextualis� par les donn�es de l'utilisateur
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <param name="message">Message de l'utilisateur</param>
    /// <param name="context">Contexte additionnel (page, stats, transactions)</param>
    /// <returns>R�ponse g�n�r�e par l'IA</returns>
    Task<string> GetChatResponseAsync(int userId, string message, string? context);

    /// <summary>
    /// D�tecte les transactions inhabituelles (anomalies)
    /// </summary>
    /// <returns>Liste des transactions suspectes avec explication</returns>
    Task<List<string>> DetectAnomaliesAsync(int userId);

    /// <summary>
    /// G�n�re des pr�dictions budg�taires bas�es sur l'historique
    /// </summary>
    /// <param name="monthsAhead">Nombre de mois � pr�dire</param>
    /// <returns>Pr�dictions de revenus et d�penses</returns>
    Task<string> PredictBudgetAsync(int userId, int monthsAhead);

    /// <summary>
    /// Analyse le patrimoine global et g�n�re des insights strat�giques via l'IA
    /// </summary>
    /// <returns>Liste de 3 insights patrimoniaux personnalis�s</returns>
    /// <remarks>
    /// Cette m�thode :
    /// 1. R�cup�re tous les assets et transactions
    /// 2. Calcule les m�triques patrimoniales :
    ///    - Valeur totale et r�partition par type
    ///    - Revenus/d�penses mensuels moyens
    ///    - Ratios cl�s (revenus/patrimoine, assets productifs)
    /// 3. Construit un prompt structur� pour Gemini
    /// 4. G�n�re 3 insights strat�giques via l'IA
    /// 5. Retourne une liste de 3 phrases courtes (15-20 mots chacune)
    /// </remarks>
    Task<List<string>> GetPortfolioInsightsAsync(int userId);
}

