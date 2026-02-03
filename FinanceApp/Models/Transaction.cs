using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models;

/// <summary>
/// Entit� Transaction : Repr�sente une transaction financi�re (revenu ou d�pense)
/// </summary>
/// <remarks>
/// Cette classe est un "mod�le" ou "entit�" EF Core.
/// EF Core va automatiquement cr�er une table "Transactions" dans PostgreSQL
/// avec des colonnes correspondant � chaque propri�t�.
/// </remarks>
public class Transaction
{
    /// <summary>
    /// Cl� primaire : identifiant unique de la transaction
    /// </summary>
    /// <remarks>
    /// [Key] : Indique explicitement que c'est la cl� primaire (optionnel si la propri�t� s'appelle "Id")
    /// EF Core va configurer cette colonne comme SERIAL (auto-incr�ment�e) dans PostgreSQL
    /// </remarks>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Identifiant de l'utilisateur propriétaire de cette transaction
    /// </summary>
    /// <remarks>
    /// Clé étrangère vers la table Users
    /// Permet d'isoler les données par utilisateur
    /// </remarks>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Date et heure de la transaction
    /// </summary>
    /// <remarks>
    /// [Required] : Rend cette propri�t� obligatoire (NOT NULL dans la base de donn�es)
    /// DateTime sera stock� comme TIMESTAMP dans PostgreSQL
    /// </remarks>
    [Required]
    public DateTime Date { get; set; }

    /// <summary>
    /// Montant de la transaction
    /// </summary>
    /// <remarks>
    /// [Column(TypeName = "decimal(18,2)")] : Sp�cifie le type exact dans la base de donn�es
    /// - decimal(18,2) signifie : 18 chiffres au total, dont 2 apr�s la virgule
    /// - Parfait pour les montants financiers (ex: 1234567890123456.78)
    /// - Plus pr�cis que "double" ou "float" qui ont des probl�mes d'arrondi
    /// </remarks>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Description de la transaction
    /// </summary>
    /// <remarks>
    /// [MaxLength(500)] : Limite la longueur � 500 caract�res (VARCHAR(500) dans PostgreSQL)
    /// Sans MaxLength, ce serait TEXT (illimit� mais moins performant pour les index)
    /// </remarks>
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Cat�gorie de la transaction (ex: Alimentation, Transport, Salaire, etc.)
    /// </summary>
    /// <remarks>
    /// [MaxLength(100)] : Cat�gories courtes donc 100 caract�res suffisent
    /// Permet de grouper et filtrer les transactions par cat�gorie
    /// </remarks>
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Type de transaction : Revenu ou D�pense
    /// </summary>
    /// <remarks>
    /// Utilise un enum pour limiter les valeurs possibles (s�curit� et coh�rence)
    /// Stock� comme INTEGER dans PostgreSQL (0 = Income, 1 = Expense)
    /// </remarks>
    [Required]
    public TransactionType Type { get; set; }

    /// <summary>
    /// Date de cr�ation de l'enregistrement (m�tadonn�e)
    /// </summary>
    /// <remarks>
    /// Bonne pratique : tracer quand les donn�es ont �t� cr��es
    /// Utile pour l'audit et le debugging
    /// </remarks>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Méthode de paiement utilisée pour cette transaction
    /// </summary>
    /// <remarks>
    /// Permet de tracer la source des flux financiers :
    /// - Cash : espèces
    /// - BankAccount : compte bancaire
    /// - CreditCard : carte de crédit (augmente la dette)
    /// - Debit : débit automatique d'un prêt
    /// </remarks>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// ID de l'actif source (si PaymentMethod = BankAccount)
    /// </summary>
    /// <remarks>
    /// Clé étrangère nullable vers Assets
    /// Ex: Transaction payée via "Compte Courant BNP" (Asset ID 5)
    /// </remarks>
    public int? SourceAssetId { get; set; }

    /// <summary>
    /// ID du passif source (si PaymentMethod = CreditCard/Loan)
    /// </summary>
    /// <remarks>
    /// Clé étrangère nullable vers Liabilities
    /// Ex: Achat avec "Visa Premier" (Liability ID 2)
    /// </remarks>
    public int? SourceLiabilityId { get; set; }
}

/// <summary>
/// Enum pour le type de transaction
/// </summary>
/// <remarks>
/// Enum = type �num�r� avec des valeurs fixes
/// Avantages :
/// 1. Type-safe : impossible d'avoir une valeur invalide
/// 2. IntelliSense : auto-compl�tion dans l'IDE
/// 3. Stockage efficace : INTEGER au lieu de VARCHAR
/// </remarks>
public enum TransactionType
{
    /// <summary>
    /// Dépense (argent sortant)
    /// </summary>
    Expense = 0,

    /// <summary>
    /// Revenu (argent entrant)
    /// </summary>
    Income = 1
}

/// <summary>
/// Enum pour la méthode de paiement
/// </summary>
/// <remarks>
/// Définit la source du flux financier pour synchroniser automatiquement
/// le patrimoine (actifs/passifs) avec les transactions
/// </remarks>
public enum PaymentMethod
{
    /// <summary>
    /// Espèces (cash) - pas de trace dans actifs/passifs
    /// </summary>
    Cash = 0,

    /// <summary>
    /// Compte bancaire - impacte un Asset de type BankAccount
    /// </summary>
    BankAccount = 1,

    /// <summary>
    /// Carte de crédit - impacte un Liability de type CreditCard
    /// </summary>
    CreditCard = 2,

    /// <summary>
    /// Débit automatique d'un prêt - impacte un Liability
    /// </summary>
    LoanDebit = 3,

    /// <summary>
    /// Autre méthode de paiement
    /// </summary>
    Other = 4
}
