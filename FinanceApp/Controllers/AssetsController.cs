using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FinanceApp.Controllers;

/// <summary>
/// Controller API REST pour gï¿½rer les actifs/patrimoine (Assets)
/// </summary>
/// <remarks>
/// Ce controller gï¿½re les endpoints CRUD pour les actifs :
/// - Immobilier (RealEstate)
/// - Vï¿½hicules (Vehicle)
/// - Investissements (Investment)
/// - Autres (Other)
/// 
/// [ApiController] : Active les comportements API (validation auto, binding auto, etc.)
/// [Route("api/[controller]")] : Route de base = /api/assets
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AssetsController> _logger;

    /// <summary>
    /// Constructeur avec injection de dï¿½pendances
    /// </summary>
    /// <param name="context">DbContext pour accï¿½der ï¿½ la base de donnï¿½es</param>
    /// <param name="logger">Service de logging</param>
    /// <remarks>
    /// INJECTION DE Dï¿½PENDANCES :
    /// 
    /// Quand une requï¿½te HTTP arrive sur /api/assets :
    /// 1. ASP.NET Core identifie le controller : AssetsController
    /// 2. Regarde les paramï¿½tres du constructeur : ApplicationDbContext, ILogger
    /// 3. Cherche dans le conteneur DI (configurï¿½ dans Program.cs)
    /// 4. Instancie le controller avec ces dï¿½pendances
    /// 5. Appelle la mï¿½thode d'action correspondante
    /// 6. ï¿½ la fin de la requï¿½te, dispose le DbContext (Scoped)
    /// </remarks>
    public AssetsController(ApplicationDbContext context, ILogger<AssetsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/assets
    /// Rï¿½cupï¿½re la liste de tous les actifs/patrimoine
    /// </summary>
    /// <returns>Liste des actifs (Asset[])</returns>
    /// <remarks>
    /// FLUX DE DONNï¿½ES :
    /// 1. Requï¿½te GET arrive : /api/assets
    /// 2. EF Core gï¿½nï¿½re le SQL : SELECT * FROM "Assets"
    /// 3. PostgreSQL exï¿½cute la requï¿½te
    /// 4. EF Core mappe les rï¿½sultats en List&lt;Asset&gt;
    /// 5. ASP.NET Core sï¿½rialise en JSON
    /// 6. Retourne HTTP 200 OK avec le JSON
    /// 
    /// EXEMPLE DE Rï¿½PONSE :
    /// [
    ///   {
    ///     "id": 1,
    ///     "name": "Appartement Paris 15e",
    ///     "value": 320000.00,
    ///     "type": "RealEstate",
    ///     "acquisitionDate": "2020-06-15T00:00:00Z"
    ///   },
    ///   {
    ///     "id": 2,
    ///     "name": "Renault Clio",
    ///     "value": 12000.00,
    ///     "type": "Vehicle",
    ///     "acquisitionDate": "2022-03-10T00:00:00Z"
    ///   }
    /// ]
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Asset>>> GetAssets([FromQuery] int userId)
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

        try
        {
            _logger.LogInformation("RÃ©cupÃ©ration de tous les actifs pour l'utilisateur {UserId}", userId);

            // ToListAsync() : RequÃªte asynchrone vers PostgreSQL
            // SELECT * FROM "Assets" WHERE "UserId" = @userId
            // Filtre par utilisateur pour l'isolation des donnÃ©es
            var assets = await _context.Assets
                .Where(a => a.UserId == userId)
                .ToListAsync();

            _logger.LogInformation("RÃ©cupÃ©ration rÃ©ussie pour l'utilisateur {UserId} : {Count} actifs", userId, assets.Count);

            // Ok() : Retourne HTTP 200 avec le JSON
            return Ok(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la rï¿½cupï¿½ration des actifs");
            return StatusCode(500, new { error = "Erreur serveur lors de la rï¿½cupï¿½ration des actifs" });
        }
    }

    /// <summary>
    /// GET /api/assets/{id}
    /// Rï¿½cupï¿½re un actif spï¿½cifique par son ID
    /// </summary>
    /// <param name="id">ID de l'actif</param>
    /// <returns>L'actif correspondant ou 404 Not Found</returns>
    /// <remarks>
    /// FLUX DE DONNï¿½ES :
    /// 1. Requï¿½te GET : /api/assets/5
    /// 2. EF Core gï¿½nï¿½re : SELECT * FROM "Assets" WHERE "Id" = 5
    /// 3. Si trouvï¿½ : retourne HTTP 200 + JSON
    /// 4. Si non trouvï¿½ : retourne HTTP 404 Not Found
    /// 
    /// CODES HTTP POSSIBLES :
    /// - 200 OK : Actif trouvï¿½
    /// - 404 Not Found : Actif inexistant
    /// - 500 Internal Server Error : Erreur serveur
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<ActionResult<Asset>> GetAsset(int id, [FromQuery] int userId)
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

        try
        {
            _logger.LogInformation("Recherche de l'actif {Id} pour l'utilisateur {UserId}", id, userId);

            // FindAsync() : Recherche par clÃ© primaire
            // SELECT * FROM "Assets" WHERE "Id" = @id
            var asset = await _context.Assets.FindAsync(id);

            if (asset == null)
            {
                _logger.LogWarning("Actif {Id} non trouvÃ© pour l'utilisateur {UserId}", id, userId);
                return NotFound(new { error = $"Actif avec ID {id} introuvable" });
            }

            // VÃ©rifier que l'actif appartient Ã  cet utilisateur
            if (asset.UserId != userId)
            {
                _logger.LogWarning(
                    "Tentative d'accÃ¨s non autorisÃ©e : Actif {Id} appartient Ã  l'utilisateur {AssetUserId}, pas Ã  {RequestedUserId}", 
                    id, 
                    asset.UserId, 
                    userId
                );
                // Retourner 403 Forbidden
                return Forbid();
            }

            _logger.LogInformation("Actif {Id} trouvï¿½ : {Name}", id, asset.Name);
            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la rï¿½cupï¿½ration de l'actif {Id}", id);
            return StatusCode(500, new { error = "Erreur serveur lors de la rï¿½cupï¿½ration de l'actif" });
        }
    }

    /// <summary>
    /// POST /api/assets
    /// Crï¿½e un nouvel actif dans le patrimoine
    /// </summary>
    /// <param name="asset">Donnï¿½es de l'actif ï¿½ crï¿½er</param>
    /// <returns>L'actif crï¿½ï¿½ avec son ID gï¿½nï¿½rï¿½</returns>
    /// <remarks>
    /// FLUX DE DONNï¿½ES :
    /// 1. Requï¿½te POST avec JSON dans le body
    /// 2. ASP.NET Core dï¿½sï¿½rialise le JSON en objet Asset
    /// 3. Validation automatique via [ApiController]
    /// 4. EF Core ajoute l'actif au contexte
    /// 5. SaveChangesAsync() gï¿½nï¿½re le SQL INSERT
    /// 6. PostgreSQL insï¿½re et retourne l'ID auto-incrï¿½mentï¿½
    /// 7. EF Core met ï¿½ jour l'objet avec le nouvel ID
    /// 8. Retourne HTTP 201 Created avec Location header
    /// 
    /// EXEMPLE DE REQUï¿½TE :
    /// POST /api/assets
    /// {
    ///   "name": "Appartement Marseille",
    ///   "value": 280000,
    ///   "type": "RealEstate",
    ///   "acquisitionDate": "2023-01-20T00:00:00Z"
    /// }
    /// 
    /// CODES HTTP POSSIBLES :
    /// - 201 Created : Actif crï¿½ï¿½ avec succï¿½s
    /// - 400 Bad Request : Donnï¿½es invalides
    /// - 500 Internal Server Error : Erreur serveur
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<Asset>> PostAsset([FromBody] Asset asset, [FromQuery] int userId)
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

        try
        {
            _logger.LogInformation("Crï¿½ation d'un nouvel actif : {Name}", asset.Name);

            asset.UserId = userId;

            // Add() : Ajoute l'actif au contexte EF Core (en mï¿½moire)
            _context.Assets.Add(asset);

            // SaveChangesAsync() : Persiste en base de donnï¿½es
            // INSERT INTO "Assets" (Name, Value, Type, AcquisitionDate)
            // VALUES (@name, @value, @type, @acquisitionDate)
            // RETURNING "Id";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Actif crï¿½ï¿½ avec ID {Id}", asset.Id);

            // CreatedAtAction() : Retourne HTTP 201 Created
            // Avec header Location: /api/assets/{id}
            // Et le JSON de l'actif crï¿½ï¿½ dans le body
            return CreatedAtAction(
                nameof(GetAsset),
                new { id = asset.Id },
                asset
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la crï¿½ation de l'actif");
            return StatusCode(500, new { error = "Erreur serveur lors de la crï¿½ation de l'actif" });
        }
    }

    /// <summary>
    /// PUT /api/assets/{id}
    /// Modifie un actif existant
    /// </summary>
    /// <param name="id">ID de l'actif ï¿½ modifier</param>
    /// <param name="asset">Nouvelles donnï¿½es de l'actif</param>
    /// <returns>HTTP 204 No Content si succï¿½s</returns>
    /// <remarks>
    /// FLUX DE DONNï¿½ES :
    /// 1. Vï¿½rification : l'ID dans l'URL doit correspondre ï¿½ l'ID dans le JSON
    /// 2. EF Core marque l'entitï¿½ comme modifiï¿½e
    /// 3. SaveChangesAsync() gï¿½nï¿½re le SQL UPDATE
    /// 4. PostgreSQL met ï¿½ jour la ligne
    /// 5. Retourne HTTP 204 No Content (succï¿½s sans body)
    /// 
    /// EXEMPLE DE REQUï¿½TE :
    /// PUT /api/assets/5
    /// {
    ///   "id": 5,
    ///   "name": "Appartement Marseille (rï¿½novï¿½)",
    ///   "value": 310000,
    ///   "type": "RealEstate",
    ///   "acquisitionDate": "2023-01-20T00:00:00Z"
    /// }
    /// 
    /// CODES HTTP POSSIBLES :
    /// - 204 No Content : Modification rï¿½ussie
    /// - 400 Bad Request : ID incohï¿½rent
    /// - 404 Not Found : Actif inexistant
    /// - 500 Internal Server Error : Erreur serveur
    /// </remarks>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsset(int id, [FromBody] Asset asset, [FromQuery] int userId)
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

        // Vï¿½rification : l'ID de l'URL doit correspondre ï¿½ l'ID du JSON
        if (id != asset.Id)
        {
            _logger.LogWarning("Tentative de modification avec ID incohï¿½rent : URL={UrlId}, Body={BodyId}", id, asset.Id);
            return BadRequest(new { error = "L'ID dans l'URL ne correspond pas ï¿½ l'ID de l'actif" });
        }

        try
        {
            _logger.LogInformation("Mise à jour de l'actif {Id} pour l'utilisateur {UserId}", id, userId);

            // Vérifier que l'actif appartient à cet utilisateur
            var existingAsset = await _context.Assets.FindAsync(id);
            if (existingAsset != null && existingAsset.UserId != userId)
            {
                _logger.LogWarning(
                    "Tentative de modification non autorisée : Actif {Id} appartient à l'utilisateur {AssetUserId}, pas à {RequestedUserId}", 
                    id, 
                    existingAsset.UserId, 
                    userId
                );
                return Forbid();
            }

            if (existingAsset != null)
            {
                asset.UserId = existingAsset.UserId;
            }

            // Entry().State : Indique ï¿½ EF Core que l'entitï¿½ est modifiï¿½e
            // EF Core va gï¿½nï¿½rer un UPDATE avec tous les champs
            _context.Entry(asset).State = EntityState.Modified;

            // SaveChangesAsync() : Gï¿½nï¿½re et exï¿½cute le SQL UPDATE
            // UPDATE "Assets" SET Name = @name, Value = @value, ...
            // WHERE "Id" = @id
            await _context.SaveChangesAsync();

            _logger.LogInformation("Actif {Id} modifiï¿½ avec succï¿½s", id);

            // NoContent() : HTTP 204 No Content (succï¿½s sans body)
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Gestion des conflits de concurrence
            // (si l'actif a ï¿½tï¿½ supprimï¿½ entre-temps)
            if (!AssetExists(id))
            {
                _logger.LogWarning("Actif {Id} non trouvï¿½ lors de la modification", id);
                return NotFound(new { error = $"Actif avec ID {id} introuvable" });
            }
            else
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la modification de l'actif {Id}", id);
            return StatusCode(500, new { error = "Erreur serveur lors de la modification de l'actif" });
        }
    }

    /// <summary>
    /// DELETE /api/assets/{id}
    /// Supprime un actif du patrimoine
    /// </summary>
    /// <param name="id">ID de l'actif ï¿½ supprimer</param>
    /// <returns>HTTP 204 No Content si succï¿½s</returns>
    /// <remarks>
    /// FLUX DE DONNï¿½ES :
    /// 1. Recherche de l'actif par ID
    /// 2. Si trouvï¿½ : EF Core marque pour suppression
    /// 3. SaveChangesAsync() gï¿½nï¿½re le SQL DELETE
    /// 4. PostgreSQL supprime la ligne
    /// 5. Retourne HTTP 204 No Content
    /// 
    /// CODES HTTP POSSIBLES :
    /// - 204 No Content : Suppression rï¿½ussie
    /// - 404 Not Found : Actif inexistant
    /// - 500 Internal Server Error : Erreur serveur
    /// </remarks>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id, [FromQuery] int userId)
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

        try
        {
            _logger.LogInformation("Suppression de l'actif {Id} pour l'utilisateur {UserId}", id, userId);

            // FindAsync() : Recherche par clï¿½ primaire
            var asset = await _context.Assets.FindAsync(id);

            if (asset == null)
            {
                _logger.LogWarning("Actif {Id} non trouvï¿½ pour suppression", id);
                return NotFound(new { error = $"Actif avec ID {id} introuvable" });
            }

            if (asset.UserId != userId)
            {
                _logger.LogWarning(
                    "Tentative de suppression non autorisï¿½e : Actif {Id} appartient ï¿½ l'utilisateur {AssetUserId}, pas ï¿½ {RequestedUserId}",
                    id,
                    asset.UserId,
                    userId
                );
                return Forbid();
            }

            // Remove() : Marque l'entitï¿½ pour suppression
            _context.Assets.Remove(asset);

            // SaveChangesAsync() : Gï¿½nï¿½re et exï¿½cute le SQL DELETE
            // DELETE FROM "Assets" WHERE "Id" = @id
            await _context.SaveChangesAsync();

            _logger.LogInformation("Actif {Id} supprimï¿½ avec succï¿½s", id);

            // NoContent() : HTTP 204 No Content
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'actif {Id}", id);
            return StatusCode(500, new { error = "Erreur serveur lors de la suppression de l'actif" });
        }
    }

    /// <summary>
    /// GET /api/assets/total-value
    /// Calcule la valeur totale du patrimoine
    /// </summary>
    /// <returns>Somme des valeurs de tous les actifs</returns>
    /// <remarks>
    /// FLUX DE DONNï¿½ES :
    /// 1. EF Core gï¿½nï¿½re : SELECT SUM("Value") FROM "Assets"
    /// 2. PostgreSQL calcule la somme
    /// 3. Retourne le montant total en dï¿½cimal
    /// 
    /// EXEMPLE DE Rï¿½PONSE :
    /// 532000.00
    /// 
    /// Si aucun actif : retourne 0
    /// 
    /// UTILITï¿½ :
    /// - Dashboard : afficher la valeur totale du patrimoine
    /// - Rapports financiers
    /// - Suivi de l'ï¿½volution du patrimoine
    /// </remarks>
    [HttpGet("total-value")]
    public async Task<ActionResult<decimal>> GetTotalValue([FromQuery] int userId)
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

        try
        {
            _logger.LogInformation("Calcul de la valeur totale du patrimoine");

            // SumAsync() : Calcule la somme directement en base de donnï¿½es
            // SELECT SUM("CurrentValue") FROM "Assets"
            // Performant : le calcul est fait par PostgreSQL, pas en C#
            var totalValue = await _context.Assets
                .Where(a => a.UserId == userId)
                .SumAsync(a => a.CurrentValue);

            _logger.LogInformation("Valeur totale du patrimoine : {TotalValue}ï¿½", totalValue);

            // Ok() : HTTP 200 avec le montant en JSON
            return Ok(totalValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul de la valeur totale");
            return StatusCode(500, new { error = "Erreur serveur lors du calcul de la valeur totale" });
        }
    }

    /// <summary>
    /// Mï¿½thode helper privï¿½e : vï¿½rifie si un actif existe
    /// </summary>
    /// <param name="id">ID de l'actif</param>
    /// <returns>True si l'actif existe, False sinon</returns>
    /// <remarks>
    /// Utilisï¿½ pour gï¿½rer les conflits de concurrence dans PUT
    /// 
    /// AnyAsync() : Gï¿½nï¿½re SELECT EXISTS(SELECT 1 FROM "Assets" WHERE "Id" = @id)
    /// Performant : retourne dï¿½s qu'une ligne est trouvï¿½e
    /// </remarks>
    private bool AssetExists(int id)
    {
        return _context.Assets.Any(e => e.Id == id);
    }

    private int? GetUserIdFromToken()
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(claimValue, out var userId) ? userId : null;
    }
}









