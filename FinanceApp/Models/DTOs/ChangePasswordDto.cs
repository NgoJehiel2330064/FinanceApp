namespace FinanceApp.Models.DTOs;

/// <summary>
/// DTO pour le changement de mot de passe
/// </summary>
public class ChangePasswordDto
{
    public int UserId { get; set; }
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}
