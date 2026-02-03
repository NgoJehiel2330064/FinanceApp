# STRATÉGIE D'EXTENSION DU PATRIMOINE - IMPLÉMENTATION

## ✅ PHASE 1 : MODÈLE DE DONNÉES (TERMINÉ)

### Création Table `Liabilities`
- ✅ Modèle C# créé (`Models/Liability.cs`)
- ✅ Types : CreditCard, Mortgage, CarLoan, PersonalLoan, StudentLoan, Other
- ✅ Champs : CurrentBalance, CreditLimit, InterestRate, MonthlyPayment, MaturityDate

### Extension Table `Transactions`
- ✅ Ajout champ `PaymentMethod` (enum : Cash, BankAccount, CreditCard, LoanDebit, Other)
- ✅ Ajout champ `SourceAssetId` (FK vers Assets, nullable)
- ✅ Ajout champ `SourceLiabilityId` (FK vers Liabilities, nullable)

### Migration Base de Données
- ✅ Migration créée : `AddLiabilitiesAndPaymentMethods`
- ✅ Migration appliquée : table `Liabilities` + colonnes transactions + index

---

## ✅ PHASE 2 : SERVICES BACKEND (TERMINÉ)

### NetWorthService (`Services/NetWorthService.cs`)
- ✅ Interface `INetWorthService`
- ✅ Méthode `CalculateNetWorthAsync(userId)` :
  - Calcule Total Actifs, Total Passifs, Patrimoine Net
  - Calcule Actifs Liquides
  - Calcule Utilisation Crédit (%)
  - Retourne répartition par type (AssetBreakdown, LiabilityBreakdown)

- ✅ Méthode `SyncTransactionImpactAsync(transaction, operation)` :
  - **BankAccount** : Revenu → +Solde | Dépense → -Solde
  - **CreditCard** : Dépense → +Dette | Paiement → -Dette
  - **LoanDebit** : Paiement → -Dette
  - **Cash/Other** : Pas d'impact tracké

- ✅ Enregistrement dans `Program.cs` : `AddScoped<INetWorthService, NetWorthService>()`

### Controllers
- ✅ `LiabilitiesController.cs` : CRUD complet pour passifs
  - GET /api/liabilities (liste)
  - GET /api/liabilities/{id} (détail)
  - POST /api/liabilities (créer)
  - PUT /api/liabilities/{id} (modifier)
  - DELETE /api/liabilities/{id} (supprimer)
  - GET /api/liabilities/total-debt (total dettes)

- ✅ `NetWorthController.cs` :
  - GET /api/networth (patrimoine complet avec répartition)

---

## 🚧 PHASE 3 : INTÉGRATION TRANSACTIONS (EN COURS)

### Modifications TransactionsController
- ✅ Injection `INetWorthService` dans constructeur
- ⏳ Hook après `POST /api/transactions` (Create)
  → Appeler `_netWorthService.SyncTransactionImpactAsync(transaction, TransactionOperation.Create)`
- ⏳ Hook après `PUT /api/transactions/{id}` (Update)
  → Appeler `_netWorthService.SyncTransactionImpactAsync(transaction, TransactionOperation.Update)`
- ⏳ Hook après `DELETE /api/transactions/{id}` (Delete)
  → Appeler `_netWorthService.SyncTransactionImpactAsync(transaction, TransactionOperation.Delete)`

---

## ⏰ PHASE 4 : FRONTEND (À VENIR)

### Types TypeScript
- Créer `types/liability.ts`
- Créer `types/payment-method.ts`
- Étendre `types/transaction.ts` avec `paymentMethod`, `sourceAssetId`, `sourceLiabilityId`

### Services Frontend
- `lib/liability-service.ts` (similaire à `asset-service.ts`)
- `lib/networth-service.ts` pour récupérer patrimoine net

### Composants Patrimoine
- Modifier `app/patrimoine/page.tsx` :
  - Ajouter onglet/section "Dettes"
  - Afficher KPIs : Total Actifs, Total Dettes, Patrimoine Net, Utilisation Crédit (%)
- Créer `components/LiabilityCard.tsx`
- Créer `components/LiabilityModal.tsx` (formulaire ajout/édition)
- Créer `components/NetWorthDashboard.tsx` (vue d'ensemble complète)

### Formulaire Transaction
- Modifier `app/transactions/page.tsx` :
  - Ajouter sélecteur "Méthode de paiement" (dropdown)
  - Si BankAccount → sélecteur "Compte" (liste Assets type BankAccount)
  - Si CreditCard → sélecteur "Carte" (liste Liabilities type CreditCard)
  - Si LoanDebit → sélecteur "Prêt" (liste Liabilities non-CreditCard)

### Synchronisation Visuelle
- Après ajout transaction, rafraîchir automatiquement :
  - Liste transactions
  - Patrimoine (actifs/passifs impactés)
  - KPIs (patrimoine net, etc.)

---

## 🎯 EXEMPLE RÉEL D'UTILISATION

### Scénario : Salaire et Dépenses

**Étape 1 : Configuration initiale**
- Utilisateur crée un Asset "Compte Courant" (Type: BankAccount, CurrentValue: 1000 $)
- Utilisateur crée une Liability "Visa Premier" (Type: CreditCard, CurrentBalance: 500 $, CreditLimit: 5000 $)

**État initial :**
- Total Actifs : 1000 $
- Total Passifs : 500 $
- **Patrimoine Net : 500 $**

**Étape 2 : Revenu**
- Transaction : +2000 $ (Type: Income, PaymentMethod: BankAccount, SourceAssetId: [Compte Courant])
- **Synchronisation automatique** → Compte Courant passe à 3000 $

**État après revenu :**
- Total Actifs : 3000 $
- Total Passifs : 500 $
- **Patrimoine Net : 2500 $**

**Étape 3 : Dépense avec carte**
- Transaction : -200 $ (Type: Expense, PaymentMethod: CreditCard, SourceLiabilityId: [Visa Premier])
- **Synchronisation automatique** → Visa Premier passe à 700 $ de dette

**État après achat :**
- Total Actifs : 3000 $
- Total Passifs : 700 $
- **Patrimoine Net : 2300 $**
- Utilisation Crédit : 700 / 5000 = **14%**

**Étape 4 : Paiement carte**
- Transaction : -500 $ (Type: Expense, Category: "Paiement carte", PaymentMethod: BankAccount, SourceAssetId: [Compte Courant])
- **Puis** : Transaction Paiement Carte (ou ajustement manuel Liability)
- Alternative : Créer transaction Type Income sur la carte (remboursement)

---

## 📋 CHECKLIST FINALE

### Backend
- [x] Modèle Liability
- [x] Extension Transaction (PaymentMethod, SourceAssetId, SourceLiabilityId)
- [x] Migration DB
- [x] NetWorthService
- [x] LiabilitiesController
- [x] NetWorthController
- [ ] Hooks dans TransactionsController (Create/Update/Delete)
- [ ] Tests API (Postman/Thunder Client)

### Frontend
- [ ] Types TypeScript (Liability, PaymentMethod)
- [ ] Services (liability-service, networth-service)
- [ ] Composants (LiabilityCard, LiabilityModal, NetWorthDashboard)
- [ ] Extension formulaire Transaction (sélecteurs)
- [ ] Page Patrimoine avec Dettes
- [ ] KPIs Dashboard

### Documentation
- [x] Ce fichier stratégie
- [ ] README mise à jour
- [ ] Guide utilisateur (comment tracker correctement)

---

## 🚨 POINTS D'ATTENTION

1. **Gestion des Updates** : Pour `TransactionOperation.Update`, le service recalcule ou nécessite l'ancienne valeur
   - **Solution choisie** : Warning + recalcul complet recommandé (complexe de gérer diff)

2. **Paiements de Carte** : Deux approches possibles
   - **Option A** : Transaction Expense (débit compte) + Transaction Income (remboursement carte)
   - **Option B** : Transaction Expense avec catégorie "Paiement carte" + logique spéciale
   - **Choisi** : Option A (plus clair)

3. **Valeurs initiales** : L'utilisateur doit saisir les soldes initiaux (Assets et Liabilities)
   - Les transactions ne modifieront que les Assets de type `BankAccount` et les Liabilities
   - Les autres actifs (immobilier, investissements) doivent être mis à jour manuellement

4. **Multi-devises** : Pas géré pour le moment (toutes valeurs en CAD)
   - Extension future : conversion automatique

---

## 🔄 PROCHAINES ÉTAPES

1. **Terminer Phase 3** : Ajouter hooks synchronisation dans TransactionsController
2. **Tester Backend** : Vérifier que la sync fonctionne correctement
3. **Phase 4** : Frontend complet
4. **Tests end-to-end** : Scénario complet utilisateur
5. **Commit & Push** sur branche `feature/patrimoine-avance`
