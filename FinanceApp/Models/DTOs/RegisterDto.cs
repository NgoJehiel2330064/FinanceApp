using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models.DTOs;

/// <summary>
/// DTO pour l'inscription d'un nouvel utilisateur
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Nom complet de l'utilisateur
    /// </summary>
    [Required(ErrorMessage = "Le nom est requis")]
    [MinLength(2, ErrorMessage = "Le nom doit contenir au moins 2 caractères")]
    [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
    public string Nom { get; set; } = string.Empty;

    /// <summary>
    /// Adresse email
    /// </summary>
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mot de passe
    /// </summary>
    [Required(ErrorMessage = "Le mot de passe est requis")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
        ErrorMessage = "Le mot de passe doit contenir au moins une majuscule, une minuscule et un chiffre")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation du mot de passe
    /// </summary>
    [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
    [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
