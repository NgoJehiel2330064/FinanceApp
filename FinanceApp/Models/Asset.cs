using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models;

/// <summary>
/// Entité Asset : Représente un actif dans le patrimoine (bien, investissement, compte bancaire)
/// </summary>
/// <remarks>
/// Cette classe permet de suivre l'évolution du patrimoine :
/// - Comptes bancaires (courant, épargne)
/// - Investissements (actions, crypto, immobilier)
/// - Biens physiques (voiture, maison)
/// </remarks>
public class Asset
{
    /// <summary>
    /// Clé primaire : identifiant unique de l'actif
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Nom de l'actif (ex: "Compte Courant BNP", "Appartement Paris", "Portefeuille Crypto")
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type d'actif pour la catégorisation
    /// </summary>
    /// <remarks>
    /// Utilise un enum pour garantir des valeurs cohérentes
    /// Facilite les rapports et statistiques par type d'actif
    /// </remarks>
    [Required]
    public AssetType Type { get; set; }

    /// <summary>
    /// Valeur actuelle de l'actif
    /// </summary>
    /// <remarks>
    /// decimal(18,2) : Précision suffisante pour des gros montants
    /// Ex: maison à 500,000.00€, compte à 15,432.50€
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
    /// Nullable (?) car certains actifs n'ont pas de valeur d'achat (héritage, cadeau)
    /// </remarks>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PurchaseValue { get; set; }

    /// <summary>
    /// Date d'acquisition de l'actif (optionnelle)
    /// </summary>
    /// <remarks>
    /// Utile pour calculer la durée de détention
    /// Important pour la fiscalité (certains gains sont exonérés après X années)
    /// </remarks>
    public DateTime? PurchaseDate { get; set; }

    /// <summary>
    /// Devise de l'actif (EUR, USD, BTC, etc.)
    /// </summary>
    /// <remarks>
    /// Important pour les actifs en devises étrangères ou crypto-monnaies
    /// Permet de gérer un patrimoine multi-devises
    /// MaxLength(10) car les codes devises sont courts (USD, EUR, BTC, ETH)
    /// </remarks>
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Description ou notes sur l'actif
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Indique si l'actif est liquide (facilement convertible en cash)
    /// </summary>
    /// <remarks>
    /// true : actifs liquides (compte bancaire, actions cotées)
    /// false : actifs non liquides (immobilier, objets de collection)
    /// Utile pour calculer la liquidité totale du patrimoine
    /// </remarks>
    public bool IsLiquid { get; set; } = true;

    /// <summary>
    /// Date de dernière mise à jour de la valeur
    /// </summary>
    /// <remarks>
    /// Important car la valeur des actifs change (actions, crypto, immobilier)
    /// Permet de savoir si la valeur est à jour
    /// </remarks>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de création de l'enregistrement
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Types d'actifs possibles
/// </summary>
/// <remarks>
/// Catégorisation standard pour l'analyse patrimoniale
/// Facilite les rapports et graphiques par catégorie d'actif
/// </remarks>
public enum AssetType
{
    /// <summary>
    /// Compte bancaire (courant, épargne)
    /// </summary>
    BankAccount = 0,

    /// <summary>
    /// Investissements financiers (actions, obligations, ETF)
    /// </summary>
    Investment = 1,

    /// <summary>
    /// Immobilier (résidence principale, investissement locatif)
    /// </summary>
    RealEstate = 2,

    /// <summary>
    /// Crypto-monnaies (Bitcoin, Ethereum, etc.)
    /// </summary>
    Cryptocurrency = 3,

    /// <summary>
    /// Véhicules (voiture, moto, bateau)
    /// </summary>
    Vehicle = 4,

    /// <summary>
    /// Autres actifs (objets d'art, bijoux, collections)
    /// </summary>
    Other = 5
}
