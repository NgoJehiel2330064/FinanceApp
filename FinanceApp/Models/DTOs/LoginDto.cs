using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models.DTOs;

/// <summary>
/// DTO pour la connexion d'un utilisateur
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Adresse email
    /// </summary>
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mot de passe
    /// </summary>
    [Required(ErrorMessage = "Le mot de passe est requis")]
    public string Password { get; set; } = string.Empty;
}
