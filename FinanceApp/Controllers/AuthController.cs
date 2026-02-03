using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Models.DTOs;
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

    public AuthController(ApplicationDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
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

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Connexion réussie",
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

    #endregion
}
