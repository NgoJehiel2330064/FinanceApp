using Microsoft.EntityFrameworkCore;
using FinanceApp.Models; // Import pour acc�der aux mod�les Transaction et Asset

namespace FinanceApp.Data;

/// <summary>
/// ApplicationDbContext : Classe centrale pour interagir avec la base de donn�es
/// Elle h�rite de DbContext (fourni par Entity Framework Core)
/// </summary>
/// <remarks>
/// Le DbContext fait plusieurs choses importantes :
/// 1. G�re la connexion � la base de donn�es
/// 2. Permet de faire des requ�tes (SELECT, INSERT, UPDATE, DELETE)
/// 3. Suit les changements sur les entit�s (Change Tracking)
/// 4. Sauvegarde les modifications dans la base de donn�es
/// </remarks>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Constructeur : re�oit les options de configuration (comme la cha�ne de connexion)
    /// </summary>
    /// <param name="options">Options pass�es depuis Program.cs lors de l'enregistrement du service</param>
    /// <remarks>
    /// Le mot-cl� 'base(options)' appelle le constructeur de la classe parent (DbContext)
    /// et lui transmet les options. C'est essentiel pour que EF Core sache comment se connecter.
    /// </remarks>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    /// <summary>
    /// DbSet repr�sentant la table "Transactions" dans PostgreSQL
    /// </summary>
    /// <remarks>
    /// DbSet<Transaction> permet de :
    /// - Faire des requ�tes LINQ : _context.Transactions.Where(t => t.Amount > 100)
    /// - Ajouter des entit�s : _context.Transactions.Add(newTransaction)
    /// - Supprimer : _context.Transactions.Remove(transaction)
    /// - Modifier : EF Core d�tecte automatiquement les changements
    /// 
    /// Le nom "Transactions" deviendra le nom de la table (convention EF Core)
    /// </remarks>
    public DbSet<Transaction> Transactions { get; set; }

    /// <summary>
    /// DbSet repr�sentant la table "Assets" dans PostgreSQL
    /// </summary>
    /// <remarks>
    /// M�me principe que Transactions mais pour les actifs du patrimoine
    /// </remarks>
    public DbSet<Asset> Assets { get; set; }
    /// <summary>
    /// DbSet représentant la table "Users" dans PostgreSQL
    /// </summary>
    /// <remarks>
    /// Gère les utilisateurs de l'application (inscription, connexion)
    /// </remarks>
    public DbSet<User> Users { get; set; }
    /// <summary>
    /// OnModelCreating : M�thode appel�e lors de la cr�ation du mod�le de donn�es
    /// C'est ici qu'on configure les relations, contraintes, index, etc.
    /// </summary>
    /// <remarks>
    /// Cette m�thode est optionnelle mais tr�s utile pour :
    /// - D�finir des cl�s primaires composites
    /// - Configurer les relations entre entit�s (one-to-many, many-to-many)
    /// - Ajouter des index pour am�liorer les performances
    /// - D�finir des valeurs par d�faut
    /// - Personnaliser les noms de tables et colonnes
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuration de l'entit� Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            // Index sur la date pour acc�l�rer les requ�tes par p�riode
            // Ex: "Toutes les transactions du mois dernier"
            // Sans index, PostgreSQL doit scanner toute la table
            // Avec index, il trouve directement les lignes concern�es
            entity.HasIndex(t => t.Date)
                  .HasDatabaseName("IX_Transactions_Date");

            // Index sur la cat�gorie pour acc�l�rer les groupements
            // Ex: "Total des d�penses par cat�gorie"
            entity.HasIndex(t => t.Category)
                  .HasDatabaseName("IX_Transactions_Category");

            // Index composite (multi-colonnes) sur Type et Date
            // Optimise les requ�tes comme : "Tous les revenus de janvier 2024"
            entity.HasIndex(t => new { t.Type, t.Date })
                  .HasDatabaseName("IX_Transactions_Type_Date");

            // Configuration de la pr�cision d�cimale pour Amount
            // HasPrecision(18, 2) : 18 chiffres au total, 2 apr�s la virgule
            // Garantit qu'on ne perd pas de pr�cision sur les montants
            entity.Property(t => t.Amount)
                  .HasPrecision(18, 2);
        });

        // Configuration de l'entit� Asset
        modelBuilder.Entity<Asset>(entity =>
        {
            // Index sur le type d'actif pour filtrer rapidement
            // Ex: "Tous mes investissements" ou "Toutes mes cryptos"
            entity.HasIndex(a => a.Type)
                  .HasDatabaseName("IX_Assets_Type");

            // Index sur la devise pour g�rer les multi-devises
            // Ex: "Tous mes actifs en USD"
            entity.HasIndex(a => a.Currency)
                  .HasDatabaseName("IX_Assets_Currency");

            // Configuration de la pr�cision pour CurrentValue
            entity.Property(a => a.CurrentValue)
                  .HasPrecision(18, 2);

            // Configuration de la pr�cision pour PurchaseValue
            // IsRequired(false) car cette valeur est nullable (?)
            entity.Property(a => a.PurchaseValue)
                  .HasPrecision(18, 2)
                  .IsRequired(false);
        });
        // Configuration de l'entité User
        modelBuilder.Entity<User>(entity =>
        {
            // Index unique sur l'email pour garantir l'unicité
            // Empêche deux utilisateurs d'avoir le même email
            entity.HasIndex(u => u.Email)
                  .IsUnique()
                  .HasDatabaseName("IX_Users_Email");

            // Index sur IsActive pour filtrer les comptes actifs
            entity.HasIndex(u => u.IsActive)
                  .HasDatabaseName("IX_Users_IsActive");

            // Configuration de l'email en minuscules
            entity.Property(u => u.Email)
                  .HasMaxLength(255)
                  .IsRequired();

            // Configuration du nom
            entity.Property(u => u.Nom)
                  .HasMaxLength(100)
                  .IsRequired();

            // Configuration du hash de mot de passe
            entity.Property(u => u.PasswordHash)
                  .IsRequired();
        });    }
}
