using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Models.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FinanceApp.Controllers;

/// <summary>
/// Contrôleur pour gérer l'authentification (inscription et connexion)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, ILogger<AuthController> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Inscription d'un nouvel utilisateur
    /// POST: api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            // Vérifier si l'email existe déjà
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == registerDto.Email.ToLower());

            if (existingUser != null)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Un compte avec cet email existe déjà"
                });
            }

            // Créer le hash du mot de passe
            var passwordHash = HashPassword(registerDto.Password);

            // Créer le nouvel utilisateur
            var user = new User
            {
                Nom = registerDto.Nom,
                Email = registerDto.Email.ToLower(),
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nouvel utilisateur créé: {Email}", user.Email);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Compte créé avec succès",
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Nom = user.Nom,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'inscription");
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "Une erreur est survenue lors de l'inscription"
            });
        }
    }

    /// <summary>
    /// Connexion d'un utilisateur
    /// POST: api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            // Rechercher l'utilisateur par email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email ou mot de passe incorrect"
                });
            }

            // Vérifier le mot de passe
            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email ou mot de passe incorrect"
                });
            }

            // Vérifier si le compte est actif
            if (!user.IsActive)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Ce compte a été désactivé"
                });
            }

            // Mettre à jour la date de dernière connexion
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Utilisateur connecté: {Email}", user.Email);

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Connexion réussie",
                Token = token,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Nom = user.Nom,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion");
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "Une erreur est survenue lors de la connexion"
            });
        }
    }

    /// <summary>
    /// Vérifier si un email est déjà utilisé
    /// GET: api/auth/check-email?email=test@example.com
    /// </summary>
    [HttpGet("check-email")]
    public async Task<ActionResult<bool>> CheckEmail([FromQuery] string email)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());

        return Ok(new { exists });
    }

    /// <summary>
    /// Obtenir le profil de l'utilisateur
    /// GET: api/auth/profile?userId=1
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> GetProfile([FromQuery] int userId)
    {
        try
        {
            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Token invalide"
                });
            }

            if (userId > 0 && userId != tokenUserId.Value)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(tokenUserId.Value);

            if (user == null || !user.IsActive)
            {
                return NotFound(new AuthResponseDto
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                });
            }

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Profil récupéré avec succès",
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Nom = user.Nom,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du profil");
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "Une erreur est survenue"
            });
        }
    }

    /// <summary>
    /// Changer le mot de passe
    /// POST: api/auth/change-password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Token invalide"
                });
            }

            if (changePasswordDto.UserId > 0 && changePasswordDto.UserId != tokenUserId.Value)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(tokenUserId.Value);

            if (user == null || !user.IsActive)
            {
                return NotFound(new AuthResponseDto
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                });
            }

            // Vérifier le mot de passe actuel
            if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Le mot de passe actuel est incorrect"
                });
            }

            // Créer le nouveau hash
            var newPasswordHash = HashPassword(changePasswordDto.NewPassword);

            // Mettre à jour
            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Mot de passe changé pour: {Email}", user.Email);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Mot de passe changé avec succès"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du changement de mot de passe");
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "Une erreur est survenue"
            });
        }
    }

    /// <summary>
    /// Supprimer le compte utilisateur
    /// DELETE: api/auth/delete-account?userId=1&password=XXX
    /// </summary>
    [HttpDelete("delete-account")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> DeleteAccount([FromQuery] int userId, [FromQuery] string password)
    {
        try
        {
            var tokenUserId = GetUserIdFromToken();
            if (tokenUserId == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Token invalide"
                });
            }

            if (userId > 0 && userId != tokenUserId.Value)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(tokenUserId.Value);

            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    Success = false,
                    Message = "Utilisateur non trouvé"
                });
            }

            // Vérifier le mot de passe pour confirmer
            if (!VerifyPassword(password, user.PasswordHash))
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Le mot de passe est incorrect"
                });
            }

            // Désactiver le compte au lieu de le supprimer (soft delete)
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Compte désactivé: {Email}", user.Email);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Compte supprimé avec succès"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du compte");
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "Une erreur est survenue"
            });
        }
    }

    #region Méthodes privées pour le hachage

    /// <summary>
    /// Créer un hash du mot de passe avec SHA256
    /// Note: Pour une application de production, utiliser BCrypt ou Argon2
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// Vérifier si le mot de passe correspond au hash
    /// </summary>
    private static bool VerifyPassword(string password, string hash)
    {
        var passwordHash = HashPassword(password);
        return passwordHash == hash;
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expiresMinutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var minutes) ? minutes : 60;

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("JWT Key is not configured.");
        }

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Nom),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int? GetUserIdFromToken()
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(claimValue, out var userId) ? userId : null;
    }

    #endregion
}
