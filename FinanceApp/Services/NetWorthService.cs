using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services;

/// <summary>
/// Interface pour le service de calcul et synchronisation du patrimoine net
/// </summary>
public interface INetWorthService
{
    Task<NetWorthSummary> CalculateNetWorthAsync(int userId);
    Task SyncTransactionImpactAsync(Transaction transaction, TransactionOperation operation);
}

/// <summary>
/// Service central pour gérer le patrimoine net (Net Worth)
/// </summary>
/// <remarks>
/// RESPONSABILITÉS :
/// 1. Calculer le patrimoine net : Total Actifs - Total Passifs
/// 2. Synchroniser automatiquement les impacts des transactions sur actifs/passifs
/// 3. Garantir la cohérence des données financières
/// 
/// LOGIQUE DE SYNCHRONISATION :
/// - Transaction Revenu + BankAccount → Augmente solde du compte
/// - Transaction Dépense + BankAccount → Diminue solde du compte
/// - Transaction Dépense + CreditCard → Augmente dette de la carte
/// - Paiement CreditCard → Diminue dette de la carte
/// </remarks>
public class NetWorthService : INetWorthService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NetWorthService> _logger;

    public NetWorthService(ApplicationDbContext context, ILogger<NetWorthService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Calcule le patrimoine net complet d'un utilisateur
    /// </summary>
    public async Task<NetWorthSummary> CalculateNetWorthAsync(int userId)
    {
        try
        {
            // Récupérer tous les actifs
            var assets = await _context.Assets
                .Where(a => a.UserId == userId)
                .ToListAsync();

            // Récupérer tous les passifs
            var liabilities = await _context.Liabilities
                .Where(l => l.UserId == userId)
                .ToListAsync();

            // CALCUL DU SOLDE NET DES TRANSACTIONS (liquidité automatique)
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var transactionNetBalance = transactions
                .Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount);

            _logger.LogInformation(
                "Solde net des transactions pour utilisateur {UserId}: {Balance:C}",
                userId, transactionNetBalance
            );

            var totalAssets = assets.Sum(a => a.CurrentValue) + transactionNetBalance;
            var totalLiabilities = liabilities.Sum(l => l.CurrentBalance);
            var netWorth = totalAssets - totalLiabilities;

            // Calculs supplémentaires
            var liquidAssets = assets.Where(a => a.IsLiquid).Sum(a => a.CurrentValue) + transactionNetBalance;
            var creditCards = liabilities.Where(l => l.Type == LiabilityType.CreditCard).ToList();
            var totalCreditUsed = creditCards.Sum(c => c.CurrentBalance);
            var totalCreditLimit = creditCards.Sum(c => c.CreditLimit ?? 0);
            var creditUtilization = totalCreditLimit > 0 
                ? (totalCreditUsed / totalCreditLimit) * 100 
                : 0;

            _logger.LogInformation(
                "Patrimoine calculé pour utilisateur {UserId}: Actifs={TotalAssets:C} (dont {TransactionBalance:C} de transactions), Passifs={TotalLiabilities:C}, Net={NetWorth:C}",
                userId, totalAssets, transactionNetBalance, totalLiabilities, netWorth
            );

            return new NetWorthSummary
            {
                UserId = userId,
                TotalAssets = totalAssets,
                TotalLiabilities = totalLiabilities,
                NetWorth = netWorth,
                LiquidAssets = liquidAssets,
                TransactionBalance = transactionNetBalance,  // Retourner le solde des transactions
                CreditUtilization = (double)creditUtilization,
                AssetBreakdown = assets.GroupBy(a => a.Type)
                    .ToDictionary(g => g.Key.ToString(), g => g.Sum(a => a.CurrentValue)),
                LiabilityBreakdown = liabilities.GroupBy(l => l.Type)
                    .ToDictionary(g => g.Key.ToString(), g => g.Sum(l => l.CurrentBalance)),
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul du patrimoine pour l'utilisateur {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Synchronise l'impact d'une transaction sur les actifs/passifs
    /// </summary>
    /// <param name="transaction">Transaction à synchroniser</param>
    /// <param name="operation">Type d'opération (Create, Update, Delete)</param>
    public async Task SyncTransactionImpactAsync(Transaction transaction, TransactionOperation operation)
    {
        try
        {
            // Si pas de méthode de paiement spécifiée, pas de sync
            if (transaction.PaymentMethod == null)
            {
                _logger.LogDebug("Transaction {TransactionId} sans méthode de paiement, pas de synchronisation", transaction.Id);
                return;
            }

            _logger.LogInformation(
                "Synchronisation transaction {TransactionId}: Montant={Amount:C}, Type={Type}, Méthode={Method}, Opération={Operation}",
                transaction.Id, transaction.Amount, transaction.Type, transaction.PaymentMethod, operation
            );

            switch (transaction.PaymentMethod)
            {
                case Models.PaymentMethod.BankAccount:
                    await SyncBankAccountTransactionAsync(transaction, operation);
                    break;

                case Models.PaymentMethod.CreditCard:
                    await SyncCreditCardTransactionAsync(transaction, operation);
                    break;

                case Models.PaymentMethod.LoanDebit:
                    await SyncLoanDebitTransactionAsync(transaction, operation);
                    break;

                case Models.PaymentMethod.Cash:
                case Models.PaymentMethod.Other:
                    // Pas d'impact sur actifs/passifs trackés
                    _logger.LogDebug("Transaction en cash/autre, pas d'impact sur patrimoine tracké");
                    break;
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation de la transaction {TransactionId}", transaction.Id);
            throw;
        }
    }

    /// <summary>
    /// Synchronise une transaction de compte bancaire
    /// </summary>
    private async Task SyncBankAccountTransactionAsync(Transaction transaction, TransactionOperation operation)
    {
        if (transaction.SourceAssetId == null)
        {
            _logger.LogWarning("Transaction {TransactionId} avec méthode BankAccount mais sans SourceAssetId", transaction.Id);
            return;
        }

        var asset = await _context.Assets.FindAsync(transaction.SourceAssetId.Value);
        if (asset == null)
        {
            _logger.LogWarning("Asset {AssetId} introuvable pour transaction {TransactionId}", transaction.SourceAssetId, transaction.Id);
            return;
        }

        if (asset.Type != AssetType.BankAccount)
        {
            _logger.LogWarning("Asset {AssetId} n'est pas un compte bancaire (Type={Type})", asset.Id, asset.Type);
            return;
        }

        switch (operation)
        {
            case TransactionOperation.Create:
                // Appliquer l'impact
                if (transaction.Type == TransactionType.Income)
                {
                    asset.CurrentValue += Math.Abs(transaction.Amount);
                    _logger.LogInformation("Revenu: +{Amount:C} sur compte {AssetName}", transaction.Amount, asset.Name);
                }
                else // Expense
                {
                    asset.CurrentValue -= Math.Abs(transaction.Amount);
                    _logger.LogInformation("Dépense: -{Amount:C} sur compte {AssetName}", transaction.Amount, asset.Name);
                }
                break;

            case TransactionOperation.Delete:
                // Annuler l'impact
                if (transaction.Type == TransactionType.Income)
                {
                    asset.CurrentValue -= Math.Abs(transaction.Amount);
                    _logger.LogInformation("Annulation revenu: -{Amount:C} sur compte {AssetName}", transaction.Amount, asset.Name);
                }
                else
                {
                    asset.CurrentValue += Math.Abs(transaction.Amount);
                    _logger.LogInformation("Annulation dépense: +{Amount:C} sur compte {AssetName}", transaction.Amount, asset.Name);
                }
                break;

            case TransactionOperation.Update:
                // Pour update, on doit avoir l'ancienne valeur - complexe
                // Simplified: recalculer depuis toutes les transactions
                _logger.LogWarning("Update de transaction nécessite recalcul complet du solde");
                break;
        }

        asset.LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Synchronise une transaction de carte de crédit
    /// </summary>
    private async Task SyncCreditCardTransactionAsync(Transaction transaction, TransactionOperation operation)
    {
        if (transaction.SourceLiabilityId == null)
        {
            _logger.LogWarning("Transaction {TransactionId} avec méthode CreditCard mais sans SourceLiabilityId", transaction.Id);
            return;
        }

        var liability = await _context.Liabilities.FindAsync(transaction.SourceLiabilityId.Value);
        if (liability == null)
        {
            _logger.LogWarning("Liability {LiabilityId} introuvable pour transaction {TransactionId}", transaction.SourceLiabilityId, transaction.Id);
            return;
        }

        if (liability.Type != LiabilityType.CreditCard)
        {
            _logger.LogWarning("Liability {LiabilityId} n'est pas une carte de crédit (Type={Type})", liability.Id, liability.Type);
            return;
        }

        switch (operation)
        {
            case TransactionOperation.Create:
                if (transaction.Type == TransactionType.Expense)
                {
                    // Dépense avec carte → augmente la dette
                    liability.CurrentBalance += Math.Abs(transaction.Amount);
                    _logger.LogInformation("Achat carte: +{Amount:C} de dette sur {CardName}", transaction.Amount, liability.Name);
                }
                else // Income (remboursement)
                {
                    // Paiement de carte → diminue la dette
                    liability.CurrentBalance -= Math.Abs(transaction.Amount);
                    _logger.LogInformation("Paiement carte: -{Amount:C} de dette sur {CardName}", transaction.Amount, liability.Name);
                }
                break;

            case TransactionOperation.Delete:
                if (transaction.Type == TransactionType.Expense)
                {
                    liability.CurrentBalance -= Math.Abs(transaction.Amount);
                    _logger.LogInformation("Annulation achat carte: -{Amount:C} sur {CardName}", transaction.Amount, liability.Name);
                }
                else
                {
                    liability.CurrentBalance += Math.Abs(transaction.Amount);
                    _logger.LogInformation("Annulation paiement carte: +{Amount:C} sur {CardName}", transaction.Amount, liability.Name);
                }
                break;

            case TransactionOperation.Update:
                _logger.LogWarning("Update de transaction carte nécessite recalcul");
                break;
        }

        liability.LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Synchronise un débit automatique de prêt
    /// </summary>
    private async Task SyncLoanDebitTransactionAsync(Transaction transaction, TransactionOperation operation)
    {
        if (transaction.SourceLiabilityId == null)
        {
            _logger.LogWarning("Transaction {TransactionId} avec méthode LoanDebit mais sans SourceLiabilityId", transaction.Id);
            return;
        }

        var liability = await _context.Liabilities.FindAsync(transaction.SourceLiabilityId.Value);
        if (liability == null)
        {
            _logger.LogWarning("Liability {LiabilityId} introuvable", transaction.SourceLiabilityId);
            return;
        }

        // Paiement de prêt → diminue toujours la dette
        switch (operation)
        {
            case TransactionOperation.Create:
                liability.CurrentBalance -= Math.Abs(transaction.Amount);
                _logger.LogInformation("Paiement prêt: -{Amount:C} sur {LoanName}", transaction.Amount, liability.Name);
                break;

            case TransactionOperation.Delete:
                liability.CurrentBalance += Math.Abs(transaction.Amount);
                _logger.LogInformation("Annulation paiement prêt: +{Amount:C} sur {LoanName}", transaction.Amount, liability.Name);
                break;
        }

        liability.LastUpdated = DateTime.UtcNow;
    }
}

/// <summary>
/// Type d'opération sur une transaction
/// </summary>
public enum TransactionOperation
{
    Create,
    Update,
    Delete
}
