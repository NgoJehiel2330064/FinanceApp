using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Models;

namespace FinanceApp.Controllers;

/// <summary>
/// Controller API REST pour gérer les actifs/patrimoine (Assets)
/// </summary>
/// <remarks>
/// Ce controller gère les endpoints CRUD pour les actifs :
/// - Immobilier (RealEstate)
/// - Véhicules (Vehicle)
/// - Investissements (Investment)
/// - Autres (Other)
/// 
/// [ApiController] : Active les comportements API (validation auto, binding auto, etc.)
/// [Route("api/[controller]")] : Route de base = /api/assets
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AssetsController> _logger;

    /// <summary>
    /// Constructeur avec injection de dépendances
    /// </summary>
    /// <param name="context">DbContext pour accéder à la base de données</param>
    /// <param name="logger">Service de logging</param>
    /// <remarks>
    /// INJECTION DE DÉPENDANCES :
    /// 
    /// Quand une requête HTTP arrive sur /api/assets :
    /// 1. ASP.NET Core identifie le controller : AssetsController
    /// 2. Regarde les paramètres du constructeur : ApplicationDbContext, ILogger
    /// 3. Cherche dans le conteneur DI (configuré dans Program.cs)
    /// 4. Instancie le controller avec ces dépendances
    /// 5. Appelle la méthode d'action correspondante
    /// 6. À la fin de la requête, dispose le DbContext (Scoped)
    /// </remarks>
    public AssetsController(ApplicationDbContext context, ILogger<AssetsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/assets
    /// Récupère la liste de tous les actifs/patrimoine
    /// </summary>
    /// <returns>Liste des actifs (Asset[])</returns>
    /// <remarks>
    /// FLUX DE DONNÉES :
    /// 1. Requête GET arrive : /api/assets
    /// 2. EF Core génère le SQL : SELECT * FROM "Assets"
    /// 3. PostgreSQL exécute la requête
    /// 4. EF Core mappe les résultats en List&lt;Asset&gt;
    /// 5. ASP.NET Core sérialise en JSON
    /// 6. Retourne HTTP 200 OK avec le JSON
    /// 
    /// EXEMPLE DE RÉPONSE :
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
    public async Task<ActionResult<IEnumerable<Asset>>> GetAssets()
    {
        try
        {
            _logger.LogInformation("Récupération de tous les actifs");

            // ToListAsync() : Requête asynchrone vers PostgreSQL
            // SELECT * FROM "Assets"
            var assets = await _context.Assets.ToListAsync();

            _logger.LogInformation("Récupération de {Count} actifs", assets.Count);

            // Ok() : Retourne HTTP 200 avec le JSON
            return Ok(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des actifs");
            return StatusCode(500, new { error = "Erreur serveur lors de la récupération des actifs" });
        }
    }

    /// <summary>
    /// GET /api/assets/{id}
    /// Récupère un actif spécifique par son ID
    /// </summary>
    /// <param name="id">ID de l'actif</param>
    /// <returns>L'actif correspondant ou 404 Not Found</returns>
    /// <remarks>
    /// FLUX DE DONNÉES :
    /// 1. Requête GET : /api/assets/5
    /// 2. EF Core génère : SELECT * FROM "Assets" WHERE "Id" = 5
    /// 3. Si trouvé : retourne HTTP 200 + JSON
    /// 4. Si non trouvé : retourne HTTP 404 Not Found
    /// 
    /// CODES HTTP POSSIBLES :
    /// - 200 OK : Actif trouvé
    /// - 404 Not Found : Actif inexistant
    /// - 500 Internal Server Error : Erreur serveur
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<ActionResult<Asset>> GetAsset(int id)
    {
        try
        {
            _logger.LogInformation("Récupération de l'actif avec ID {Id}", id);

            // FindAsync() : Recherche par clé primaire
            // SELECT * FROM "Assets" WHERE "Id" = @id
            var asset = await _context.Assets.FindAsync(id);

            if (asset == null)
            {
                _logger.LogWarning("Actif {Id} non trouvé", id);
                return NotFound(new { error = $"Actif avec ID {id} introuvable" });
            }

            _logger.LogInformation("Actif {Id} trouvé : {Name}", id, asset.Name);
            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'actif {Id}", id);
            return StatusCode(500, new { error = "Erreur serveur lors de la récupération de l'actif" });
        }
    }

    /// <summary>
    /// POST /api/assets
    /// Crée un nouvel actif dans le patrimoine
    /// </summary>
    /// <param name="asset">Données de l'actif à créer</param>
    /// <returns>L'actif créé avec son ID généré</returns>
    /// <remarks>
    /// FLUX DE DONNÉES :
    /// 1. Requête POST avec JSON dans le body
    /// 2. ASP.NET Core désérialise le JSON en objet Asset
    /// 3. Validation automatique via [ApiController]
    /// 4. EF Core ajoute l'actif au contexte
    /// 5. SaveChangesAsync() génère le SQL INSERT
    /// 6. PostgreSQL insère et retourne l'ID auto-incrémenté
    /// 7. EF Core met à jour l'objet avec le nouvel ID
    /// 8. Retourne HTTP 201 Created avec Location header
    /// 
    /// EXEMPLE DE REQUÊTE :
    /// POST /api/assets
    /// {
    ///   "name": "Appartement Marseille",
    ///   "value": 280000,
    ///   "type": "RealEstate",
    ///   "acquisitionDate": "2023-01-20T00:00:00Z"
    /// }
    /// 
    /// CODES HTTP POSSIBLES :
    /// - 201 Created : Actif créé avec succès
    /// - 400 Bad Request : Données invalides
    /// - 500 Internal Server Error : Erreur serveur
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<Asset>> PostAsset(Asset asset)
    {
        try
        {
            _logger.LogInformation("Création d'un nouvel actif : {Name}", asset.Name);

            // Add() : Ajoute l'actif au contexte EF Core (en mémoire)
            _context.Assets.Add(asset);

            // SaveChangesAsync() : Persiste en base de données
            // INSERT INTO "Assets" (Name, Value, Type, AcquisitionDate)
            // VALUES (@name, @value, @type, @acquisitionDate)
            // RETURNING "Id";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Actif créé avec ID {Id}", asset.Id);

            // CreatedAtAction() : Retourne HTTP 201 Created
            // Avec header Location: /api/assets/{id}
            // Et le JSON de l'actif créé dans le body
            return CreatedAtAction(
                nameof(GetAsset),
                new { id = asset.Id },
                asset
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'actif");
            return StatusCode(500, new { error = "Erreur serveur lors de la création de l'actif" });
        }
    }

    /// <summary>
    /// PUT /api/assets/{id}
    /// Modifie un actif existant
    /// </summary>
    /// <param name="id">ID de l'actif à modifier</param>
    /// <param name="asset">Nouvelles données de l'actif</param>
    /// <returns>HTTP 204 No Content si succès</returns>
    /// <remarks>
    /// FLUX DE DONNÉES :
    /// 1. Vérification : l'ID dans l'URL doit correspondre à l'ID dans le JSON
    /// 2. EF Core marque l'entité comme modifiée
    /// 3. SaveChangesAsync() génère le SQL UPDATE
    /// 4. PostgreSQL met à jour la ligne
    /// 5. Retourne HTTP 204 No Content (succès sans body)
    /// 
    /// EXEMPLE DE REQUÊTE :
    /// PUT /api/assets/5
    /// {
    ///   "id": 5,
    ///   "name": "Appartement Marseille (rénové)",
    ///   "value": 310000,
    ///   "type": "RealEstate",
    ///   "acquisitionDate": "2023-01-20T00:00:00Z"
    /// }
    /// 
    /// CODES HTTP POSSIBLES :
    /// - 204 No Content : Modification réussie
    /// - 400 Bad Request : ID incohérent
    /// - 404 Not Found : Actif inexistant
    /// - 500 Internal Server Error : Erreur serveur
    /// </remarks>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsset(int id, Asset asset)
    {
        // Vérification : l'ID de l'URL doit correspondre à l'ID du JSON
        if (id != asset.Id)
        {
            _logger.LogWarning("Tentative de modification avec ID incohérent : URL={UrlId}, Body={BodyId}", id, asset.Id);
            return BadRequest(new { error = "L'ID dans l'URL ne correspond pas à l'ID de l'actif" });
        }

        try
        {
            _logger.LogInformation("Modification de l'actif {Id}", id);

            // Entry().State : Indique à EF Core que l'entité est modifiée
            // EF Core va générer un UPDATE avec tous les champs
            _context.Entry(asset).State = EntityState.Modified;

            // SaveChangesAsync() : Génère et exécute le SQL UPDATE
            // UPDATE "Assets" SET Name = @name, Value = @value, ...
            // WHERE "Id" = @id
            await _context.SaveChangesAsync();

            _logger.LogInformation("Actif {Id} modifié avec succès", id);

            // NoContent() : HTTP 204 No Content (succès sans body)
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Gestion des conflits de concurrence
            // (si l'actif a été supprimé entre-temps)
            if (!AssetExists(id))
            {
                _logger.LogWarning("Actif {Id} non trouvé lors de la modification", id);
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
    /// <param name="id">ID de l'actif à supprimer</param>
    /// <returns>HTTP 204 No Content si succès</returns>
    /// <remarks>
    /// FLUX DE DONNÉES :
    /// 1. Recherche de l'actif par ID
    /// 2. Si trouvé : EF Core marque pour suppression
    /// 3. SaveChangesAsync() génère le SQL DELETE
    /// 4. PostgreSQL supprime la ligne
    /// 5. Retourne HTTP 204 No Content
    /// 
    /// CODES HTTP POSSIBLES :
    /// - 204 No Content : Suppression réussie
    /// - 404 Not Found : Actif inexistant
    /// - 500 Internal Server Error : Erreur serveur
    /// </remarks>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        try
        {
            _logger.LogInformation("Suppression de l'actif {Id}", id);

            // FindAsync() : Recherche par clé primaire
            var asset = await _context.Assets.FindAsync(id);

            if (asset == null)
            {
                _logger.LogWarning("Actif {Id} non trouvé pour suppression", id);
                return NotFound(new { error = $"Actif avec ID {id} introuvable" });
            }

            // Remove() : Marque l'entité pour suppression
            _context.Assets.Remove(asset);

            // SaveChangesAsync() : Génère et exécute le SQL DELETE
            // DELETE FROM "Assets" WHERE "Id" = @id
            await _context.SaveChangesAsync();

            _logger.LogInformation("Actif {Id} supprimé avec succès", id);

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
    /// FLUX DE DONNÉES :
    /// 1. EF Core génère : SELECT SUM("Value") FROM "Assets"
    /// 2. PostgreSQL calcule la somme
    /// 3. Retourne le montant total en décimal
    /// 
    /// EXEMPLE DE RÉPONSE :
    /// 532000.00
    /// 
    /// Si aucun actif : retourne 0
    /// 
    /// UTILITÉ :
    /// - Dashboard : afficher la valeur totale du patrimoine
    /// - Rapports financiers
    /// - Suivi de l'évolution du patrimoine
    /// </remarks>
    [HttpGet("total-value")]
    public async Task<ActionResult<decimal>> GetTotalValue()
    {
        try
        {
            _logger.LogInformation("Calcul de la valeur totale du patrimoine");

            // SumAsync() : Calcule la somme directement en base de données
            // SELECT SUM("Value") FROM "Assets"
            // Performant : le calcul est fait par PostgreSQL, pas en C#
            var totalValue = await _context.Assets.SumAsync(a => a.Value);

            _logger.LogInformation("Valeur totale du patrimoine : {TotalValue}€", totalValue);

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
    /// Méthode helper privée : vérifie si un actif existe
    /// </summary>
    /// <param name="id">ID de l'actif</param>
    /// <returns>True si l'actif existe, False sinon</returns>
    /// <remarks>
    /// Utilisé pour gérer les conflits de concurrence dans PUT
    /// 
    /// AnyAsync() : Génère SELECT EXISTS(SELECT 1 FROM "Assets" WHERE "Id" = @id)
    /// Performant : retourne dès qu'une ligne est trouvée
    /// </remarks>
    private bool AssetExists(int id)
    {
        return _context.Assets.Any(e => e.Id == id);
    }
}
