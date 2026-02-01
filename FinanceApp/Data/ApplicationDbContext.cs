using Microsoft.EntityFrameworkCore;
using FinanceApp.Models; // Import pour accéder aux modèles Transaction et Asset

namespace FinanceApp.Data;

/// <summary>
/// ApplicationDbContext : Classe centrale pour interagir avec la base de données
/// Elle hérite de DbContext (fourni par Entity Framework Core)
/// </summary>
/// <remarks>
/// Le DbContext fait plusieurs choses importantes :
/// 1. Gère la connexion à la base de données
/// 2. Permet de faire des requêtes (SELECT, INSERT, UPDATE, DELETE)
/// 3. Suit les changements sur les entités (Change Tracking)
/// 4. Sauvegarde les modifications dans la base de données
/// </remarks>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Constructeur : reçoit les options de configuration (comme la chaîne de connexion)
    /// </summary>
    /// <param name="options">Options passées depuis Program.cs lors de l'enregistrement du service</param>
    /// <remarks>
    /// Le mot-clé 'base(options)' appelle le constructeur de la classe parent (DbContext)
    /// et lui transmet les options. C'est essentiel pour que EF Core sache comment se connecter.
    /// </remarks>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    /// <summary>
    /// DbSet représentant la table "Transactions" dans PostgreSQL
    /// </summary>
    /// <remarks>
    /// DbSet<Transaction> permet de :
    /// - Faire des requêtes LINQ : _context.Transactions.Where(t => t.Amount > 100)
    /// - Ajouter des entités : _context.Transactions.Add(newTransaction)
    /// - Supprimer : _context.Transactions.Remove(transaction)
    /// - Modifier : EF Core détecte automatiquement les changements
    /// 
    /// Le nom "Transactions" deviendra le nom de la table (convention EF Core)
    /// </remarks>
    public DbSet<Transaction> Transactions { get; set; }

    /// <summary>
    /// DbSet représentant la table "Assets" dans PostgreSQL
    /// </summary>
    /// <remarks>
    /// Même principe que Transactions mais pour les actifs du patrimoine
    /// </remarks>
    public DbSet<Asset> Assets { get; set; }

    /// <summary>
    /// OnModelCreating : Méthode appelée lors de la création du modèle de données
    /// C'est ici qu'on configure les relations, contraintes, index, etc.
    /// </summary>
    /// <remarks>
    /// Cette méthode est optionnelle mais très utile pour :
    /// - Définir des clés primaires composites
    /// - Configurer les relations entre entités (one-to-many, many-to-many)
    /// - Ajouter des index pour améliorer les performances
    /// - Définir des valeurs par défaut
    /// - Personnaliser les noms de tables et colonnes
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuration de l'entité Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            // Index sur la date pour accélérer les requêtes par période
            // Ex: "Toutes les transactions du mois dernier"
            // Sans index, PostgreSQL doit scanner toute la table
            // Avec index, il trouve directement les lignes concernées
            entity.HasIndex(t => t.Date)
                  .HasDatabaseName("IX_Transactions_Date");

            // Index sur la catégorie pour accélérer les groupements
            // Ex: "Total des dépenses par catégorie"
            entity.HasIndex(t => t.Category)
                  .HasDatabaseName("IX_Transactions_Category");

            // Index composite (multi-colonnes) sur Type et Date
            // Optimise les requêtes comme : "Tous les revenus de janvier 2024"
            entity.HasIndex(t => new { t.Type, t.Date })
                  .HasDatabaseName("IX_Transactions_Type_Date");

            // Configuration de la précision décimale pour Amount
            // HasPrecision(18, 2) : 18 chiffres au total, 2 après la virgule
            // Garantit qu'on ne perd pas de précision sur les montants
            entity.Property(t => t.Amount)
                  .HasPrecision(18, 2);
        });

        // Configuration de l'entité Asset
        modelBuilder.Entity<Asset>(entity =>
        {
            // Index sur le type d'actif pour filtrer rapidement
            // Ex: "Tous mes investissements" ou "Toutes mes cryptos"
            entity.HasIndex(a => a.Type)
                  .HasDatabaseName("IX_Assets_Type");

            // Index sur la devise pour gérer les multi-devises
            // Ex: "Tous mes actifs en USD"
            entity.HasIndex(a => a.Currency)
                  .HasDatabaseName("IX_Assets_Currency");

            // Configuration de la précision pour CurrentValue
            entity.Property(a => a.CurrentValue)
                  .HasPrecision(18, 2);

            // Configuration de la précision pour PurchaseValue
            // IsRequired(false) car cette valeur est nullable (?)
            entity.Property(a => a.PurchaseValue)
                  .HasPrecision(18, 2)
                  .IsRequired(false);
        });
    }
}
