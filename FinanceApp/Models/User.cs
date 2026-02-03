using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models;

/// <summary>
/// Modèle User représentant un utilisateur de l'application
/// </summary>
public class User
{
    /// <summary>
    /// Identifiant unique de l'utilisateur (clé primaire)
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Nom complet de l'utilisateur
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    /// <summary>
    /// Adresse email (unique pour chaque utilisateur)
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash du mot de passe (jamais stocker en clair)
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Date de création du compte
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de dernière mise à jour
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indique si le compte est actif
    /// </summary>
    public bool IsActive { get; set; } = true;
}
