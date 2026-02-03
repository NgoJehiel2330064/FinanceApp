using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models;

/// <summary>
/// Entité Liability : Représente un passif/dette dans le patrimoine
/// </summary>
/// <remarks>
/// Les passifs réduisent le patrimoine net :
/// - Cartes de crédit
/// - Prêts (auto, immo, personnel)
/// - Autres dettes
/// 
/// Patrimoine Net = Total Actifs - Total Passifs
/// </remarks>
public class Liability
{
    /// <summary>
    /// Clé primaire : identifiant unique du passif
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Identifiant de l'utilisateur propriétaire de ce passif
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Nom du passif (ex: "Carte Visa", "Prêt Auto", "Hypothèque")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type de passif pour la catégorisation
    /// </summary>
    [Required]
    public LiabilityType Type { get; set; }

    /// <summary>
    /// Solde actuel de la dette (montant dû)
    /// </summary>
    /// <remarks>
    /// Toujours positif : représente ce qu'on DOIT
    /// Ex: 2500.00 € de solde carte de crédit
    /// </remarks>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// Limite de crédit (pour cartes de crédit)
    /// </summary>
    /// <remarks>
    /// Nullable : uniquement pour les cartes de crédit
    /// Ex: Carte avec limite 5000 €, solde actuel 2500 € → crédit disponible 2500 €
    /// </remarks>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// Taux d'intérêt annuel (%)
    /// </summary>
    /// <remarks>
    /// Ex: 19.5 pour 19.5% par an
    /// Permet de calculer les intérêts à payer
    /// </remarks>
    [Column(TypeName = "decimal(5,2)")]
    public decimal? InterestRate { get; set; }

    /// <summary>
    /// Mensualité (paiement mensuel minimum/régulier)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MonthlyPayment { get; set; }

    /// <summary>
    /// Date d'échéance finale (pour prêts)
    /// </summary>
    public DateTime? MaturityDate { get; set; }

    /// <summary>
    /// Devise du passif
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "CAD";

    /// <summary>
    /// Description ou notes
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Date de dernière mise à jour du solde
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de création de l'enregistrement
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Types de passifs possibles
/// </summary>
public enum LiabilityType
{
    /// <summary>
    /// Carte de crédit
    /// </summary>
    CreditCard = 0,

    /// <summary>
    /// Prêt hypothécaire (immobilier)
    /// </summary>
    Mortgage = 1,

    /// <summary>
    /// Prêt automobile
    /// </summary>
    CarLoan = 2,

    /// <summary>
    /// Prêt personnel
    /// </summary>
    PersonalLoan = 3,

    /// <summary>
    /// Prêt étudiant
    /// </summary>
    StudentLoan = 4,

    /// <summary>
    /// Autre dette
    /// </summary>
    Other = 5
}
