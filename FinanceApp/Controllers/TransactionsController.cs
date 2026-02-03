using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FinanceApp.Controllers;

/// <summary>
/// Controller API REST pour g�rer les transactions financi�res
/// </summary>
/// <remarks>
/// [ApiController] : Attribut qui active des comportements sp�cifiques aux API :
/// - Validation automatique du ModelState
/// - Binding automatique des param�tres depuis [FromBody], [FromQuery], etc.
/// - R�ponses HTTP standardis�es (400, 404, 500)
/// 
/// [Route("api/[controller]")] : D�finit la route de base
/// - [controller] est remplac� par "Transactions" (nom du controller sans "Controller")
/// - Route finale : /api/transactions
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    // ApplicationDbContext : Pour acc�der � la base de donn�es
    private readonly ApplicationDbContext _context;
    
    // IGeminiService : Pour utiliser l'IA (sugg�rer cat�gories, etc.)
    private readonly IGeminiService _geminiService;
    
    // INetWorthService : Pour synchroniser transactions → patrimoine
    private readonly INetWorthService _netWorthService;
    
    // ILogger : Pour tracer les op�rations et erreurs
    private readonly ILogger<TransactionsController> _logger;

    /// <summary>
    /// Constructeur avec injection de d�pendances
    /// </summary>
    /// <remarks>
    /// INJECTION DE D�PENDANCES (DI) :
    /// 
    /// Quand une requ�te HTTP arrive, ASP.NET Core fait ceci :
    /// 1. Regarde quel controller doit traiter la requ�te (TransactionsController)
    /// 2. V�rifie les param�tres du constructeur (context, geminiService, logger)
    /// 3. Cherche dans le conteneur DI les services enregistr�s (dans Program.cs)
    /// 4. Instancie le controller en injectant les d�pendances
    /// 5. Appelle la m�thode d'action (Get, Post, etc.)
    /// 
    /// DUR�E DE VIE (Scopes) :
    /// - ApplicationDbContext : Scoped (une instance par requ�te HTTP)
    ///   -> Cr�� au d�but de la requ�te, d�truit � la fin
    ///   -> �vite les probl�mes de concurrence entre requ�tes
    /// 
    /// - IGeminiService : Scoped ou Singleton selon la configuration
    ///   -> On utilisera Scoped pour ce projet
    /// 
    /// - ILogger : Singleton (une seule instance partag�e)
    ///   -> Pas d'�tat, juste pour �crire des logs
    /// </remarks>
    public TransactionsController(
        ApplicationDbContext context,
        IGeminiService geminiService,
        INetWorthService netWorthService,
        ILogger<TransactionsController> logger)
    {
        _context = context;
        _geminiService = geminiService;
        _netWorthService = netWorthService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/transactions
    /// R�cup�re toutes les transactions
    /// </summary>
    /// <remarks>
    /// [HttpGet] : Indique que cette m�thode r�pond aux requ�tes GET
    /// 
    /// ASYNC/AWAIT :
    /// - ToListAsync() : Requ�te asynchrone � la base de donn�es
    /// - Pendant que PostgreSQL traite la requ�te, le thread est lib�r�
    /// - Permet de g�rer plus de requ�tes simultan�ment
    /// - Important pour la scalabilit� (performance sous charge)
    /// 
    /// ActionResult<T> : Type de retour pour les API
    /// - Ok(data) retourne 200 avec les donn�es en JSON
    /// - BadRequest() retourne 400
    /// - NotFound() retourne 404
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions([FromQuery] int userId)
    {
        var tokenUserId = GetUserIdFromToken();
        if (tokenUserId == null)
        {
            return Unauthorized("Token invalide");
        }

        if (userId <= 0)
        {
            userId = tokenUserId.Value;
        }
        else if (userId != tokenUserId.Value)
        {
            return Forbid();
        }

        try
        {
            _logger.LogInformation("Récupération de toutes les transactions pour l'utilisateur {UserId}", userId);

            // EF Core traduit ceci en SQL :
            // SELECT * FROM "Transactions" WHERE "UserId" = @userId ORDER BY "Date" DESC
            // Filtre par utilisateur pour l'isolation des données
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            _logger.LogInformation("Récupération réussie pour l'utilisateur {UserId} : {Count} transactions", userId, transactions.Count);

            // Ok() retourne HTTP 200 avec les donn�es en JSON
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            // En cas d'erreur (ex: base de donn�es inaccessible)
            _logger.LogError(ex, "Erreur lors de la r�cup�ration des transactions");
            
            // StatusCode(500) retourne HTTP 500 Internal Server Error
            return StatusCode(500, "Erreur lors de la r�cup�ration des transactions");
        }
    }

    /// <summary>
    /// GET /api/transactions/{id}
    /// R�cup�re une transaction sp�cifique par son ID
    /// </summary>
    /// <param name="id">ID de la transaction</param>
    /// <remarks>
    /// [HttpGet("{id}")] : Route avec param�tre dynamique
    /// - Exemple : GET /api/transactions/5
    /// - {id} dans la route correspond au param�tre "int id"
    /// 
    /// FindAsync(id) : Recherche par cl� primaire
    /// - Tr�s rapide (utilise l'index de la cl� primaire)
    /// - SQL : SELECT * FROM "Transactions" WHERE "Id" = @id
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<ActionResult<Transaction>> GetTransaction(int id, [FromQuery] int userId)
    {
        var tokenUserId = GetUserIdFromToken();
        if (tokenUserId == null)
        {
            return Unauthorized("Token invalide");
        }

        if (userId <= 0)
        {
            userId = tokenUserId.Value;
        }
        else if (userId != tokenUserId.Value)
        {
            return Forbid();
        }

        try
        {
            _logger.LogInformation("Recherche de la transaction {Id} pour l'utilisateur {UserId}", id, userId);

            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction {Id} non trouvée pour l'utilisateur {UserId}", id, userId);
                
                // NotFound() retourne HTTP 404
                return NotFound(new { message = $"Transaction {id} non trouvée" });
            }

            // Vérifier que la transaction appartient à cet utilisateur
            if (transaction.UserId != userId)
            {
                _logger.LogWarning(
                    "Tentative d'accès non autorisée : Transaction {Id} appartient à l'utilisateur {TransactionUserId}, pas à {RequestedUserId}", 
                    id, 
                    transaction.UserId, 
                    userId
                );
                // Retourner 403 Forbidden
                return Forbid();
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la r�cup�ration de la transaction {Id}", id);
            return StatusCode(500, "Erreur lors de la r�cup�ration de la transaction");
        }
    }

    /// <summary>
    /// POST /api/transactions
    /// Cr�e une nouvelle transaction
    /// </summary>
    /// <param name="transaction">Donn�es de la transaction (envoy�es dans le body JSON)</param>
    /// <remarks>
    /// [HttpPost] : R�pond aux requ�tes POST (cr�ation)
    /// 
    /// [FromBody] : Les donn�es viennent du corps de la requ�te HTTP en JSON
    /// - ASP.NET Core d�s�rialise automatiquement le JSON en objet Transaction
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
    /// - EF Core g�n�re le SQL INSERT
    /// - PostgreSQL cr�e la ligne et retourne l'ID auto-g�n�r�
    /// - L'objet transaction est mis � jour avec le nouvel ID
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] Transaction transaction, [FromQuery] int userId)
    {
        var tokenUserId = GetUserIdFromToken();
        if (tokenUserId == null)
        {
            return Unauthorized("Token invalide");
        }

        if (userId <= 0)
        {
            userId = tokenUserId.Value;
        }
        else if (userId != tokenUserId.Value)
        {
            return Forbid();
        }

        try
        {
            _logger.LogInformation(
                "Création d'une nouvelle transaction pour l'utilisateur {UserId} : {Description}, {Amount}€", 
                userId,
                transaction.Description, 
                transaction.Amount
            );

            // Définir les métadonnées et associer à l'utilisateur
            transaction.UserId = userId;
            transaction.CreatedAt = DateTime.UtcNow;

            // Add() ajoute l'entit� au contexte (pas encore en base)
            _context.Transactions.Add(transaction);

            // SaveChangesAsync() ex�cute r�ellement l'INSERT dans PostgreSQL
            // Retourne le nombre de lignes affect�es
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Transaction cr��e avec succ�s : ID={Id}", 
                transaction.Id
            );

            // SYNCHRONISATION PATRIMOINE : Mettre à jour automatiquement actifs/passifs
            try
            {
                await _netWorthService.SyncTransactionImpactAsync(transaction, TransactionOperation.Create);
                _logger.LogInformation("Synchronisation patrimoine réussie pour la transaction {Id}", transaction.Id);
            }
            catch (Exception syncEx)
            {
                _logger.LogError(syncEx, "Erreur lors de la synchronisation patrimoine pour la transaction {Id}", transaction.Id);
                // On ne bloque pas la création, mais on log l'erreur
            }

            // CreatedAtAction retourne HTTP 201 Created
            // + En-t�te Location: /api/transactions/{id}
            // + Les donn�es de la transaction dans le body
            return CreatedAtAction(
                nameof(GetTransaction),  // Nom de l'action pour construire l'URL
                new { id = transaction.Id },  // Param�tres de route
                transaction  // Donn�es � retourner
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la cr�ation de la transaction");
            return StatusCode(500, "Erreur lors de la cr�ation de la transaction");
        }
    }

    /// <summary>
    /// PUT /api/transactions/{id}
    /// Met � jour une transaction existante
    /// </summary>
    /// <param name="id">ID de la transaction � modifier</param>
    /// <param name="transaction">Nouvelles donn�es de la transaction</param>
    /// <remarks>
    /// [HttpPut("{id}")] : R�pond aux requ�tes PUT (mise � jour compl�te)
    /// 
    /// PROCESSUS :
    /// 1. V�rifier que l'ID dans l'URL correspond � l'ID dans le body
    /// 2. V�rifier que la transaction existe
    /// 3. Marquer l'entit� comme modifi�e
    /// 4. Sauvegarder (g�n�re UPDATE en SQL)
    /// </remarks>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, [FromBody] Transaction transaction, [FromQuery] int userId)
    {
        var tokenUserId = GetUserIdFromToken();
        if (tokenUserId == null)
        {
            return Unauthorized("Token invalide");
        }

        if (userId <= 0)
        {
            userId = tokenUserId.Value;
        }
        else if (userId != tokenUserId.Value)
        {
            return Forbid();
        }

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
            _logger.LogInformation("Mise à jour de la transaction {Id} pour l'utilisateur {UserId}", id, userId);

            // Vérifier que la transaction appartient à cet utilisateur
            var existingTransaction = await _context.Transactions.FindAsync(id);
            if (existingTransaction == null)
            {
                _logger.LogWarning("Transaction {Id} non trouvée", id);
                return NotFound("Transaction non trouvée");
            }
            
            if (existingTransaction.UserId != userId)
            {
                _logger.LogWarning(
                    "Tentative de modification non autorisée : Transaction {Id} appartient à l'utilisateur {TransactionUserId}, pas à {RequestedUserId}", 
                    id, 
                    existingTransaction.UserId, 
                    userId
                );
                return Forbid();
            }

            // Mettre à jour les propriétés de l'entité existante (déjà trackée par EF Core)
            // On ne crée PAS une nouvelle instance, on modifie celle qui est déjà trackée
            existingTransaction.Amount = transaction.Amount;
            existingTransaction.Description = transaction.Description;
            existingTransaction.Category = transaction.Category;
            existingTransaction.Type = transaction.Type;
            existingTransaction.Date = transaction.Date;
            // On ne modifie PAS CreatedAt ni UserId

            await _context.SaveChangesAsync();

            _logger.LogInformation("Transaction {Id} mise à jour avec succès", id);

            // SYNCHRONISATION PATRIMOINE : Mettre à jour automatiquement actifs/passifs
            try
            {
                await _netWorthService.SyncTransactionImpactAsync(existingTransaction, TransactionOperation.Update);
                _logger.LogInformation("Synchronisation patrimoine réussie pour la modification de la transaction {Id}", id);
            }
            catch (Exception syncEx)
            {
                _logger.LogError(syncEx, "Erreur lors de la synchronisation patrimoine pour la transaction {Id}", id);
                // On ne bloque pas la modification, mais on log l'erreur
            }

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
            _logger.LogError(ex, "Erreur lors de la mise � jour de la transaction {Id}", id);
            return StatusCode(500, "Erreur lors de la mise � jour");
        }
    }

    /// <summary>
    /// DELETE /api/transactions/{id}
    /// Supprime une transaction
    /// </summary>
    /// <param name="id">ID de la transaction � supprimer</param>
    /// <remarks>
    /// [HttpDelete("{id}")] : R�pond aux requ�tes DELETE
    /// 
    /// PROCESSUS :
    /// 1. V�rifier que la transaction existe
    /// 2. Remove() marque l'entit� pour suppression
    /// 3. SaveChanges() g�n�re DELETE en SQL
    /// </remarks>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id, [FromQuery] int userId)
    {
        var tokenUserId = GetUserIdFromToken();
        if (tokenUserId == null)
        {
            return Unauthorized("Token invalide");
        }

        if (userId <= 0)
        {
            userId = tokenUserId.Value;
        }
        else if (userId != tokenUserId.Value)
        {
            return Forbid();
        }

        try
        {
            _logger.LogInformation("Suppression de la transaction {Id} pour l'utilisateur {UserId}", id, userId);

            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction {Id} non trouvée pour suppression par l'utilisateur {UserId}", id, userId);
                return NotFound();
            }

            if (transaction.UserId != userId)
            {
                _logger.LogWarning(
                    "Tentative de suppression non autorisée : Transaction {Id} appartient à l'utilisateur {TransactionUserId}, pas à {RequestedUserId}",
                    id,
                    transaction.UserId,
                    userId
                );
                return Forbid();
            }

            // Vérifier que la transaction appartient à cet utilisateur
            if (transaction.UserId != userId)
            {
                _logger.LogWarning(
                    "Tentative de suppression non autorisée : Transaction {Id} appartient à l'utilisateur {TransactionUserId}, pas à {RequestedUserId}", 
                    id, 
                    transaction.UserId, 
                    userId
                );
                return Forbid();
            }

            // Remove() marque l'entit� pour suppression
            _context.Transactions.Remove(transaction);

            // SaveChanges() ex�cute le DELETE
            await _context.SaveChangesAsync();

            _logger.LogInformation("Transaction {Id} supprim�e avec succ�s", id);

            // SYNCHRONISATION PATRIMOINE : Mettre à jour automatiquement actifs/passifs
            try
            {
                await _netWorthService.SyncTransactionImpactAsync(transaction, TransactionOperation.Delete);
                _logger.LogInformation("Synchronisation patrimoine réussie pour la suppression de la transaction {Id}", id);
            }
            catch (Exception syncEx)
            {
                _logger.LogError(syncEx, "Erreur lors de la synchronisation patrimoine pour la transaction {Id}", id);
                // La transaction est déjà supprimée, on log juste l'erreur de sync
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la transaction {Id}", id);
            return StatusCode(500, "Erreur lors de la suppression");
        }
    }

    /// <summary>
    /// M�thode helper pour v�rifier si une transaction existe
    /// </summary>
    private async Task<bool> TransactionExists(int id)
    {
        // AnyAsync() : Requ�te optimis�e qui retourne juste true/false
        // Plus rapide que FindAsync() car ne charge pas les donn�es
        // SQL : SELECT EXISTS(SELECT 1 FROM "Transactions" WHERE "Id" = @id)
        return await _context.Transactions.AnyAsync(t => t.Id == id);
    }

    private int? GetUserIdFromToken()
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(claimValue, out var userId) ? userId : null;
    }
}
