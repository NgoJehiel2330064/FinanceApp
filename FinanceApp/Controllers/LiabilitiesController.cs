using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Models;
using System.Security.Claims;

namespace FinanceApp.Controllers;

/// <summary>
/// Controller API pour gérer les passifs/dettes (Liabilities)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LiabilitiesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LiabilitiesController> _logger;

    public LiabilitiesController(ApplicationDbContext context, ILogger<LiabilitiesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int? GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// GET /api/liabilities
    /// Récupère tous les passifs d'un utilisateur
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Liability>>> GetLiabilities([FromQuery] int userId)
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
            _logger.LogInformation("Récupération des passifs pour l'utilisateur {UserId}", userId);

            var liabilities = await _context.Liabilities
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CurrentBalance)
                .ToListAsync();

            _logger.LogInformation("Récupération réussie : {Count} passifs", liabilities.Count);
            return Ok(liabilities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des passifs");
            return StatusCode(500, "Erreur lors de la récupération des passifs");
        }
    }

    /// <summary>
    /// GET /api/liabilities/{id}
    /// Récupère un passif spécifique
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Liability>> GetLiability(int id, [FromQuery] int userId)
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
            var liability = await _context.Liabilities.FindAsync(id);

            if (liability == null)
            {
                _logger.LogWarning("Passif {Id} non trouvé", id);
                return NotFound(new { message = $"Passif {id} non trouvé" });
            }

            if (liability.UserId != userId)
            {
                _logger.LogWarning("Tentative d'accès non autorisée au passif {Id}", id);
                return Forbid();
            }

            return Ok(liability);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du passif {Id}", id);
            return StatusCode(500, "Erreur lors de la récupération du passif");
        }
    }

    /// <summary>
    /// POST /api/liabilities
    /// Crée un nouveau passif
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Liability>> CreateLiability([FromBody] Liability liability, [FromQuery] int userId)
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
            // Forcer l'UserId depuis le token
            liability.UserId = userId;
            liability.CreatedAt = DateTime.UtcNow;
            liability.LastUpdated = DateTime.UtcNow;

            _context.Liabilities.Add(liability);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Passif {LiabilityId} créé pour l'utilisateur {UserId}", liability.Id, userId);

            return CreatedAtAction(
                nameof(GetLiability),
                new { id = liability.Id, userId },
                liability
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du passif");
            return StatusCode(500, "Erreur lors de la création du passif");
        }
    }

    /// <summary>
    /// PUT /api/liabilities/{id}
    /// Met à jour un passif
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLiability(int id, [FromBody] Liability liability, [FromQuery] int userId)
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

        if (id != liability.Id)
        {
            return BadRequest("L'ID du passif ne correspond pas");
        }

        try
        {
            var existingLiability = await _context.Liabilities.FindAsync(id);
            if (existingLiability == null)
            {
                return NotFound("Passif non trouvé");
            }

            if (existingLiability.UserId != userId)
            {
                return Forbid();
            }

            // Mettre à jour les propriétés
            existingLiability.Name = liability.Name;
            existingLiability.Type = liability.Type;
            existingLiability.CurrentBalance = liability.CurrentBalance;
            existingLiability.CreditLimit = liability.CreditLimit;
            existingLiability.InterestRate = liability.InterestRate;
            existingLiability.MonthlyPayment = liability.MonthlyPayment;
            existingLiability.MaturityDate = liability.MaturityDate;
            existingLiability.Currency = liability.Currency;
            existingLiability.Description = liability.Description;
            existingLiability.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Passif {Id} mis à jour", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du passif {Id}", id);
            return StatusCode(500, "Erreur lors de la mise à jour");
        }
    }

    /// <summary>
    /// DELETE /api/liabilities/{id}
    /// Supprime un passif
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLiability(int id, [FromQuery] int userId)
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
            var liability = await _context.Liabilities.FindAsync(id);
            if (liability == null)
            {
                return NotFound();
            }

            if (liability.UserId != userId)
            {
                return Forbid();
            }

            _context.Liabilities.Remove(liability);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Passif {Id} supprimé", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du passif {Id}", id);
            return StatusCode(500, "Erreur lors de la suppression");
        }
    }

    /// <summary>
    /// GET /api/liabilities/total-debt
    /// Calcule le total des dettes
    /// </summary>
    [HttpGet("total-debt")]
    public async Task<ActionResult<decimal>> GetTotalDebt([FromQuery] int userId)
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
            var totalDebt = await _context.Liabilities
                .Where(l => l.UserId == userId)
                .SumAsync(l => l.CurrentBalance);

            _logger.LogInformation("Total dette calculé pour utilisateur {UserId}: {TotalDebt:C}", userId, totalDebt);
            return Ok(totalDebt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul du total des dettes");
            return StatusCode(500, "Erreur lors du calcul");
        }
    }
}
