using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models;

/// <summary>
/// Entité Transaction : Représente une transaction financière (revenu ou dépense)
/// </summary>
/// <remarks>
/// Cette classe est un "modèle" ou "entité" EF Core.
/// EF Core va automatiquement créer une table "Transactions" dans PostgreSQL
/// avec des colonnes correspondant à chaque propriété.
/// </remarks>
public class Transaction
{
    /// <summary>
    /// Clé primaire : identifiant unique de la transaction
    /// </summary>
    /// <remarks>
    /// [Key] : Indique explicitement que c'est la clé primaire (optionnel si la propriété s'appelle "Id")
    /// EF Core va configurer cette colonne comme SERIAL (auto-incrémentée) dans PostgreSQL
    /// </remarks>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Date et heure de la transaction
    /// </summary>
    /// <remarks>
    /// [Required] : Rend cette propriété obligatoire (NOT NULL dans la base de données)
    /// DateTime sera stocké comme TIMESTAMP dans PostgreSQL
    /// </remarks>
    [Required]
    public DateTime Date { get; set; }

    /// <summary>
    /// Montant de la transaction
    /// </summary>
    /// <remarks>
    /// [Column(TypeName = "decimal(18,2)")] : Spécifie le type exact dans la base de données
    /// - decimal(18,2) signifie : 18 chiffres au total, dont 2 après la virgule
    /// - Parfait pour les montants financiers (ex: 1234567890123456.78)
    /// - Plus précis que "double" ou "float" qui ont des problèmes d'arrondi
    /// </remarks>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Description de la transaction
    /// </summary>
    /// <remarks>
    /// [MaxLength(500)] : Limite la longueur à 500 caractères (VARCHAR(500) dans PostgreSQL)
    /// Sans MaxLength, ce serait TEXT (illimité mais moins performant pour les index)
    /// </remarks>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Catégorie de la transaction (ex: Alimentation, Transport, Salaire, etc.)
    /// </summary>
    /// <remarks>
    /// [MaxLength(100)] : Catégories courtes donc 100 caractères suffisent
    /// Permet de grouper et filtrer les transactions par catégorie
    /// </remarks>
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Type de transaction : Revenu ou Dépense
    /// </summary>
    /// <remarks>
    /// Utilise un enum pour limiter les valeurs possibles (sécurité et cohérence)
    /// Stocké comme INTEGER dans PostgreSQL (0 = Income, 1 = Expense)
    /// </remarks>
    [Required]
    public TransactionType Type { get; set; }

    /// <summary>
    /// Date de création de l'enregistrement (métadonnée)
    /// </summary>
    /// <remarks>
    /// Bonne pratique : tracer quand les données ont été créées
    /// Utile pour l'audit et le debugging
    /// </remarks>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enum pour le type de transaction
/// </summary>
/// <remarks>
/// Enum = type énuméré avec des valeurs fixes
/// Avantages :
/// 1. Type-safe : impossible d'avoir une valeur invalide
/// 2. IntelliSense : auto-complétion dans l'IDE
/// 3. Stockage efficace : INTEGER au lieu de VARCHAR
/// </remarks>
public enum TransactionType
{
    /// <summary>
    /// Revenu (argent entrant)
    /// </summary>
    Income = 0,

    /// <summary>
    /// Dépense (argent sortant)
    /// </summary>
    Expense = 1
}
