using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceApp.Services;
using FinanceApp.Models;
using System.Security.Claims;

namespace FinanceApp.Controllers;

/// <summary>
/// Controller pour calculer et récupérer le patrimoine net
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NetWorthController : ControllerBase
{
    private readonly INetWorthService _netWorthService;
    private readonly ILogger<NetWorthController> _logger;

    public NetWorthController(INetWorthService netWorthService, ILogger<NetWorthController> logger)
    {
        _netWorthService = netWorthService;
        _logger = logger;
    }

    private int? GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// GET /api/networth
    /// Calcule et retourne le patrimoine net complet
    /// </summary>
    /// <remarks>
    /// Retourne :
    /// - Total des actifs
    /// - Total des passifs
    /// - Patrimoine net (actifs - passifs)
    /// - Actifs liquides
    /// - Utilisation crédit (%)
    /// - Répartition par type
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<NetWorthSummary>> GetNetWorth([FromQuery] int userId)
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
            _logger.LogInformation("Calcul du patrimoine net pour l'utilisateur {UserId}", userId);
            
            var netWorth = await _netWorthService.CalculateNetWorthAsync(userId);
            
            return Ok(netWorth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul du patrimoine net pour l'utilisateur {UserId}", userId);
            return StatusCode(500, new { message = "Erreur lors du calcul du patrimoine net" });
        }
    }
}
