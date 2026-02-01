using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Services;

namespace FinanceApp.Controllers;

/// <summary>
/// Controller API REST pour gérer les transactions financières
/// </summary>
/// <remarks>
/// [ApiController] : Attribut qui active des comportements spécifiques aux API :
/// - Validation automatique du ModelState
/// - Binding automatique des paramètres depuis [FromBody], [FromQuery], etc.
/// - Réponses HTTP standardisées (400, 404, 500)
/// 
/// [Route("api/[controller]")] : Définit la route de base
/// - [controller] est remplacé par "Transactions" (nom du controller sans "Controller")
/// - Route finale : /api/transactions
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    // ApplicationDbContext : Pour accéder à la base de données
    private readonly ApplicationDbContext _context;
    
    // IGeminiService : Pour utiliser l'IA (suggérer catégories, etc.)
    private readonly IGeminiService _geminiService;
    
    // ILogger : Pour tracer les opérations et erreurs
    private readonly ILogger<TransactionsController> _logger;

    /// <summary>
    /// Constructeur avec injection de dépendances
    /// </summary>
    /// <remarks>
    /// INJECTION DE DÉPENDANCES (DI) :
    /// 
    /// Quand une requête HTTP arrive, ASP.NET Core fait ceci :
    /// 1. Regarde quel controller doit traiter la requête (TransactionsController)
    /// 2. Vérifie les paramètres du constructeur (context, geminiService, logger)
    /// 3. Cherche dans le conteneur DI les services enregistrés (dans Program.cs)
    /// 4. Instancie le controller en injectant les dépendances
    /// 5. Appelle la méthode d'action (Get, Post, etc.)
    /// 
    /// DURÉE DE VIE (Scopes) :
    /// - ApplicationDbContext : Scoped (une instance par requête HTTP)
    ///   -> Créé au début de la requête, détruit à la fin
    ///   -> Évite les problèmes de concurrence entre requêtes
    /// 
    /// - IGeminiService : Scoped ou Singleton selon la configuration
    ///   -> On utilisera Scoped pour ce projet
    /// 
    /// - ILogger : Singleton (une seule instance partagée)
    ///   -> Pas d'état, juste pour écrire des logs
    /// </remarks>
    public TransactionsController(
        ApplicationDbContext context,
        IGeminiService geminiService,
        ILogger<TransactionsController> logger)
    {
        _context = context;
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/transactions
    /// Récupère toutes les transactions
    /// </summary>
    /// <remarks>
    /// [HttpGet] : Indique que cette méthode répond aux requêtes GET
    /// 
    /// ASYNC/AWAIT :
    /// - ToListAsync() : Requête asynchrone à la base de données
    /// - Pendant que PostgreSQL traite la requête, le thread est libéré
    /// - Permet de gérer plus de requêtes simultanément
    /// - Important pour la scalabilité (performance sous charge)
    /// 
    /// ActionResult<T> : Type de retour pour les API
    /// - Ok(data) retourne 200 avec les données en JSON
    /// - BadRequest() retourne 400
    /// - NotFound() retourne 404
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
    {
        try
        {
            _logger.LogInformation("Récupération de toutes les transactions");

            // EF Core traduit ceci en SQL :
            // SELECT * FROM "Transactions" ORDER BY "Date" DESC
            // L'ordre DESC affiche les plus récentes en premier
            var transactions = await _context.Transactions
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            _logger.LogInformation("Récupération réussie : {Count} transactions", transactions.Count);

            // Ok() retourne HTTP 200 avec les données en JSON
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            // En cas d'erreur (ex: base de données inaccessible)
            _logger.LogError(ex, "Erreur lors de la récupération des transactions");
            
            // StatusCode(500) retourne HTTP 500 Internal Server Error
            return StatusCode(500, "Erreur lors de la récupération des transactions");
        }
    }

    /// <summary>
    /// GET /api/transactions/{id}
    /// Récupère une transaction spécifique par son ID
    /// </summary>
    /// <param name="id">ID de la transaction</param>
    /// <remarks>
    /// [HttpGet("{id}")] : Route avec paramètre dynamique
    /// - Exemple : GET /api/transactions/5
    /// - {id} dans la route correspond au paramètre "int id"
    /// 
    /// FindAsync(id) : Recherche par clé primaire
    /// - Très rapide (utilise l'index de la clé primaire)
    /// - SQL : SELECT * FROM "Transactions" WHERE "Id" = @id
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<ActionResult<Transaction>> GetTransaction(int id)
    {
        try
        {
            _logger.LogInformation("Recherche de la transaction {Id}", id);

            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction {Id} non trouvée", id);
                
                // NotFound() retourne HTTP 404
                return NotFound(new { message = $"Transaction {id} non trouvée" });
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la transaction {Id}", id);
            return StatusCode(500, "Erreur lors de la récupération de la transaction");
        }
    }

    /// <summary>
    /// POST /api/transactions
    /// Crée une nouvelle transaction
    /// </summary>
    /// <param name="transaction">Données de la transaction (envoyées dans le body JSON)</param>
    /// <remarks>
    /// [HttpPost] : Répond aux requêtes POST (création)
    /// 
    /// [FromBody] : Les données viennent du corps de la requête HTTP en JSON
    /// - ASP.NET Core désérialise automatiquement le JSON en objet Transaction
    /// - Exemple de JSON :
    ///   {
    ///     "date": "2024-01-15",
    ///     "amount": 50.00,
    ///     "description": "Courses Carrefour",
    ///     "category": "Alimentation",
    ///     "type": 1
    ///   }
    /// 
    /// VALIDATION :
    /// - ASP.NET Core valide automatiquement les [Required], [MaxLength], etc.
    /// - Si invalide, retourne HTTP 400 automatiquement
    /// 
    /// SaveChangesAsync() :
    /// - EF Core génère le SQL INSERT
    /// - PostgreSQL crée la ligne et retourne l'ID auto-généré
    /// - L'objet transaction est mis à jour avec le nouvel ID
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] Transaction transaction)
    {
        try
        {
            _logger.LogInformation(
                "Création d'une nouvelle transaction : {Description}, {Amount}€", 
                transaction.Description, 
                transaction.Amount
            );

            // Définir les métadonnées
            transaction.CreatedAt = DateTime.UtcNow;

            // Add() ajoute l'entité au contexte (pas encore en base)
            _context.Transactions.Add(transaction);

            // SaveChangesAsync() exécute réellement l'INSERT dans PostgreSQL
            // Retourne le nombre de lignes affectées
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Transaction créée avec succès : ID={Id}", 
                transaction.Id
            );

            // CreatedAtAction retourne HTTP 201 Created
            // + En-tête Location: /api/transactions/{id}
            // + Les données de la transaction dans le body
            return CreatedAtAction(
                nameof(GetTransaction),  // Nom de l'action pour construire l'URL
                new { id = transaction.Id },  // Paramètres de route
                transaction  // Données à retourner
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de la transaction");
            return StatusCode(500, "Erreur lors de la création de la transaction");
        }
    }

    /// <summary>
    /// PUT /api/transactions/{id}
    /// Met à jour une transaction existante
    /// </summary>
    /// <param name="id">ID de la transaction à modifier</param>
    /// <param name="transaction">Nouvelles données de la transaction</param>
    /// <remarks>
    /// [HttpPut("{id}")] : Répond aux requêtes PUT (mise à jour complète)
    /// 
    /// PROCESSUS :
    /// 1. Vérifier que l'ID dans l'URL correspond à l'ID dans le body
    /// 2. Vérifier que la transaction existe
    /// 3. Marquer l'entité comme modifiée
    /// 4. Sauvegarder (génère UPDATE en SQL)
    /// </remarks>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, [FromBody] Transaction transaction)
    {
        if (id != transaction.Id)
        {
            _logger.LogWarning(
                "Tentative de modification avec ID incohérent : URL={UrlId}, Body={BodyId}", 
                id, 
                transaction.Id
            );
            return BadRequest("L'ID de la transaction ne correspond pas");
        }

        try
        {
            _logger.LogInformation("Mise à jour de la transaction {Id}", id);

            // Entry() récupère le "tracker" de cette entité
            // State = Modified : Indique à EF Core que l'entité a changé
            // Au SaveChanges, EF Core génère un UPDATE pour toutes les propriétés
            _context.Entry(transaction).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Transaction {Id} mise à jour avec succès", id);

            // NoContent() retourne HTTP 204 (succès sans contenu)
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Exception levée si la transaction a été supprimée entre-temps
            // (conflit de concurrence)
            if (!await TransactionExists(id))
            {
                _logger.LogWarning("Transaction {Id} non trouvée lors de la mise à jour", id);
                return NotFound();
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de la transaction {Id}", id);
            return StatusCode(500, "Erreur lors de la mise à jour");
        }
    }

    /// <summary>
    /// DELETE /api/transactions/{id}
    /// Supprime une transaction
    /// </summary>
    /// <param name="id">ID de la transaction à supprimer</param>
    /// <remarks>
    /// [HttpDelete("{id}")] : Répond aux requêtes DELETE
    /// 
    /// PROCESSUS :
    /// 1. Vérifier que la transaction existe
    /// 2. Remove() marque l'entité pour suppression
    /// 3. SaveChanges() génère DELETE en SQL
    /// </remarks>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        try
        {
            _logger.LogInformation("Suppression de la transaction {Id}", id);

            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction {Id} non trouvée pour suppression", id);
                return NotFound();
            }

            // Remove() marque l'entité pour suppression
            _context.Transactions.Remove(transaction);

            // SaveChanges() exécute le DELETE
            await _context.SaveChangesAsync();

            _logger.LogInformation("Transaction {Id} supprimée avec succès", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la transaction {Id}", id);
            return StatusCode(500, "Erreur lors de la suppression");
        }
    }

    /// <summary>
    /// Méthode helper pour vérifier si une transaction existe
    /// </summary>
    private async Task<bool> TransactionExists(int id)
    {
        // AnyAsync() : Requête optimisée qui retourne juste true/false
        // Plus rapide que FindAsync() car ne charge pas les données
        // SQL : SELECT EXISTS(SELECT 1 FROM "Transactions" WHERE "Id" = @id)
        return await _context.Transactions.AnyAsync(t => t.Id == id);
    }
}
