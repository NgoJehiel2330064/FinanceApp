using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models;

/// <summary>
/// Entit� Asset : Repr�sente un actif dans le patrimoine (bien, investissement, compte bancaire)
/// </summary>
/// <remarks>
/// Cette classe permet de suivre l'�volution du patrimoine :
/// - Comptes bancaires (courant, �pargne)
/// - Investissements (actions, crypto, immobilier)
/// - Biens physiques (voiture, maison)
/// </remarks>
public class Asset
{
    /// <summary>
    /// Cl� primaire : identifiant unique de l'actif
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Identifiant de l'utilisateur propriétaire de cet actif
    /// </summary>
    /// <remarks>
    /// Clé étrangère vers la table Users
    /// Permet d'isoler les données par utilisateur
    /// </remarks>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Nom de l'actif (ex: "Compte Courant BNP", "Appartement Paris", "Portefeuille Crypto")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type d'actif pour la cat�gorisation
    /// </summary>
    /// <remarks>
    /// Utilise un enum pour garantir des valeurs coh�rentes
    /// Facilite les rapports et statistiques par type d'actif
    /// </remarks>
    [Required]
    public AssetType Type { get; set; }

    /// <summary>
    /// Valeur actuelle de l'actif
    /// </summary>
    /// <remarks>
    /// decimal(18,2) : Pr�cision suffisante pour des gros montants
    /// Ex: maison � 500,000.00�, compte � 15,432.50�
    /// 
    /// Pour les comptes bancaires (BankAccount), cette valeur représente
    /// le solde actuel qui est automatiquement synchronisé avec les transactions.
    /// </remarks>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentValue { get; set; }

    /// <summary>
    /// Valeur d'achat initiale (optionnelle)
    /// </summary>
    /// <remarks>
    /// Permet de calculer la plus-value ou moins-value
    /// Gain/Perte = CurrentValue - PurchaseValue
    /// Nullable (?) car certains actifs n'ont pas de valeur d'achat (h�ritage, cadeau)
    /// </remarks>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PurchaseValue { get; set; }

    /// <summary>
    /// Date d'acquisition de l'actif (optionnelle)
    /// </summary>
    /// <remarks>
    /// Utile pour calculer la dur�e de d�tention
    /// Important pour la fiscalit� (certains gains sont exon�r�s apr�s X ann�es)
    /// </remarks>
    public DateTime? PurchaseDate { get; set; }

    /// <summary>
    /// Devise de l'actif (EUR, USD, BTC, etc.)
    /// </summary>
    /// <remarks>
    /// Important pour les actifs en devises �trang�res ou crypto-monnaies
    /// Permet de g�rer un patrimoine multi-devises
    /// MaxLength(10) car les codes devises sont courts (USD, EUR, BTC, ETH)
    /// </remarks>
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "CAD";

    /// <summary>
    /// Description ou notes sur l'actif
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Indique si l'actif est liquide (facilement convertible en cash)
    /// </summary>
    /// <remarks>
    /// true : actifs liquides (compte bancaire, actions cot�es)
    /// false : actifs non liquides (immobilier, objets de collection)
    /// Utile pour calculer la liquidit� totale du patrimoine
    /// </remarks>
    public bool IsLiquid { get; set; } = true;

    /// <summary>
    /// Date de derni�re mise � jour de la valeur
    /// </summary>
    /// <remarks>
    /// Important car la valeur des actifs change (actions, crypto, immobilier)
    /// Permet de savoir si la valeur est � jour
    /// </remarks>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de cr�ation de l'enregistrement
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Types d'actifs possibles
/// </summary>
/// <remarks>
/// Cat�gorisation standard pour l'analyse patrimoniale
/// Facilite les rapports et graphiques par cat�gorie d'actif
/// </remarks>
public enum AssetType
{
    /// <summary>
    /// Compte bancaire (courant, �pargne)
    /// </summary>
    BankAccount = 0,

    /// <summary>
    /// Investissements financiers (actions, obligations, ETF)
    /// </summary>
    Investment = 1,

    /// <summary>
    /// Immobilier (r�sidence principale, investissement locatif)
    /// </summary>
    RealEstate = 2,

    /// <summary>
    /// Crypto-monnaies (Bitcoin, Ethereum, etc.)
    /// </summary>
    Cryptocurrency = 3,

    /// <summary>
    /// V�hicules (voiture, moto, bateau)
    /// </summary>
    Vehicle = 4,

    /// <summary>
    /// Autres actifs (objets d'art, bijoux, collections)
    /// </summary>
    Other = 5
}

/// <summary>
/// Résumé du patrimoine net avec détails
/// </summary>
public class NetWorthSummary
{
    public int UserId { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public decimal LiquidAssets { get; set; }
    public decimal TransactionBalance { get; set; }  // Solde net des transactions (revenus - dépenses)
    public double CreditUtilization { get; set; }
    public Dictionary<string, decimal> AssetBreakdown { get; set; } = new();
    public Dictionary<string, decimal> LiabilityBreakdown { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}
