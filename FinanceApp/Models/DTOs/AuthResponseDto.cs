namespace FinanceApp.Models.DTOs;

/// <summary>
/// DTO pour la réponse d'authentification (inscription ou connexion)
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Indique si l'opération a réussi
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message de résultat
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Token JWT (pour authentification future - optionnel pour l'instant)
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Informations de l'utilisateur
    /// </summary>
    public UserInfoDto? User { get; set; }
}

/// <summary>
/// Informations basiques de l'utilisateur (sans données sensibles)
/// </summary>
public class UserInfoDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
