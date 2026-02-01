# 🚀 Guide de Démarrage - Patrimoine Extension

## ✅ Ce qui a été ajouté

Votre application Finance Dashboard dispose maintenant d'une **section complète de gestion du patrimoine** sans toucher à la fonctionnalité existante des transactions.

### 📦 Nouveaux fichiers créés:
```
finance-ui/
  ├── types/asset.ts                    ← Interfaces TypeScript
  ├── components/
  │   ├── AssetCard.tsx                ← Carte d'affichage d'actif
  │   ├── AssetList.tsx                ← Liste des actifs
  │   └── AssetModal.tsx               ← Modal création/édition
  └── app/page.tsx                     ← Modifié (section ajoutée)

Documentation/
  ├── PATRIMOINE-EXTENSION.md          ← Documentation complète
  └── test-assets-api.http             ← Tests API
```

---

## 🏃 Démarrage Rapide

### 1️⃣ Démarrer le Backend (API)
```powershell
cd FinanceApp
dotnet run
```
✅ L'API démarre sur **http://localhost:5152**

### 2️⃣ Démarrer le Frontend
```powershell
cd finance-ui
npm run dev
```
✅ Le dashboard s'ouvre sur **http://localhost:3000**

### 3️⃣ Accéder au Dashboard
Ouvrez votre navigateur: **http://localhost:3000**

Vous verrez maintenant:
- **Section Transactions** (existante, inchangée)
- **Section Patrimoine** (nouvelle) 💎

---

## 🎯 Fonctionnalités Disponibles

### Types d'actifs gérés:
1. 🏦 **Comptes Bancaires** - Comptes épargne, courants, etc.
2. 📈 **Investissements** - Actions, ETF, obligations, etc.
3. 🏠 **Immobilier** - Appartements, maisons, terrains
4. ₿ **Crypto-monnaies** - Bitcoin, Ethereum, etc.
5. 🚗 **Véhicules** - Voitures, motos, bateaux
6. 💼 **Autres** - Montres, œuvres d'art, collections

### Fonctionnalités:
✅ Ajouter un actif (bouton "Ajouter un actif")  
✅ Modifier un actif (hover sur carte → icône ✏️)  
✅ Supprimer un actif (hover sur carte → icône 🗑️)  
✅ Voir le patrimoine total (calculé automatiquement)  
✅ Calcul automatique du gain/perte (si valeur d'achat fournie)  
✅ Badge "Liquide" pour actifs facilement convertibles  
✅ Format CAD (dollars canadiens)  

---

## 📝 Tester l'Application

### Option 1: Via l'Interface Web

1. Ouvrez http://localhost:3000
2. Scrollez jusqu'à "💎 Mon Patrimoine"
3. Cliquez sur "Ajouter un actif"
4. Remplissez le formulaire:
   - **Nom**: "Compte Épargne TD"
   - **Type**: Compte Bancaire
   - **Valeur actuelle**: 25000
   - **Liquidité**: ✅ Coché
5. Cliquez "Ajouter l'actif"

### Option 2: Via l'API (test-assets-api.http)

1. Ouvrez `test-assets-api.http` dans VS Code
2. Cliquez sur "Send Request" au-dessus de chaque requête
3. Testez les différents scénarios fournis

**Extension VS Code recommandée:**
- REST Client (humao.rest-client)

---

## 🎨 Design

Le design suit **strictement le même style** que les transactions:

### Glassmorphism
- Fond flou: `backdrop-blur-xl`
- Background semi-transparent: `bg-white/5`
- Bordures subtiles: `border-white/10`
- Hover effect: `hover:bg-white/10`

### Couleurs
- **Patrimoine Total**: Gradient violet/rose
- **Gain**: Vert émeraude (`text-emerald-400`)
- **Perte**: Rouge (`text-red-400`)
- **Badges**: Bleu pour liquide, gris pour notes

### Animations
- Entrée progressive des cartes (100ms de délai entre chaque)
- Modal avec animation `scaleIn`
- Hover fluide sur les boutons d'action

---

## 🔧 Configuration

### Variables d'environnement (.env.local)
```env
NEXT_PUBLIC_API_URL=http://localhost:5152
NEXT_PUBLIC_GEMINI_API_KEY=AIzaSyCpYUPvjgvhPNtCjlJDg0ddmwCXPvUZRCg
```

### Endpoints API utilisés
```
GET    /api/assets              ← Liste des actifs
GET    /api/assets/{id}         ← Un actif par ID
POST   /api/assets              ← Créer un actif
PUT    /api/assets/{id}         ← Modifier un actif
DELETE /api/assets/{id}         ← Supprimer un actif
GET    /api/assets/total-value  ← Valeur totale du patrimoine
```

---

## 🐛 Dépannage

### Le backend ne démarre pas
```powershell
# Vérifier que PostgreSQL est actif
docker ps

# Redémarrer le conteneur si nécessaire
docker-compose up -d

# Vérifier les migrations
cd FinanceApp
dotnet ef database update
```

### Le frontend ne se connecte pas à l'API
1. Vérifiez que le backend tourne sur port 5152
2. Vérifiez `.env.local`: `NEXT_PUBLIC_API_URL=http://localhost:5152`
3. Redémarrez le dev server: `npm run dev`

### Erreur CORS
Le backend est déjà configuré pour accepter `localhost:3000`.  
Si problème persiste, vérifiez `Program.cs` → `AllowFrontend` policy.

### Erreur TypeScript
```powershell
cd finance-ui
npm run build
```
Si des erreurs apparaissent, vérifiez:
- Imports manquants
- Types mal définis
- Props mal passées

---

## 📊 Exemple de Données de Test

Ajoutez ces actifs pour tester:

### Compte Bancaire
- Nom: Compte Épargne TD
- Type: Compte Bancaire (0)
- Valeur: 25,000 CAD
- Liquidité: Oui

### Investissement
- Nom: Actions Tesla
- Type: Investissement (1)
- Valeur actuelle: 45,000 CAD
- Valeur d'achat: 38,000 CAD
- Date d'achat: 2023-01-15
- Liquidité: Oui

### Immobilier
- Nom: Appartement Montréal
- Type: Immobilier (2)
- Valeur: 450,000 CAD
- Valeur d'achat: 380,000 CAD
- Date d'achat: 2020-03-01
- Liquidité: Non

**Patrimoine total attendu:** 520,000 CAD

---

## 📚 Documentation

### Documentation complète
Voir `PATRIMOINE-EXTENSION.md` pour:
- Architecture détaillée
- Composants et props
- Flux de données
- API endpoints
- Tests

### Documentation technique globale
Voir `BRIEF-TECHNIQUE-COMPLET.txt` pour:
- Vue d'ensemble du projet
- Technologies utilisées
- Configuration complète
- Troubleshooting

---

## ✨ Prochaines Étapes

### Fonctionnalités suggérées (non implémentées):
1. **Graphiques d'évolution** - Chart.js pour voir l'évolution du patrimoine
2. **Alertes de valeur** - Notifier si un actif passe un seuil
3. **Import CSV** - Importer plusieurs actifs en masse
4. **Export PDF** - Générer un rapport de patrimoine
5. **Catégories personnalisées** - Permettre d'ajouter des sous-catégories
6. **Multi-devises** - Gérer USD, EUR en plus de CAD
7. **Dashboard analytics** - Répartition par type (pie chart)

### Pour aller plus loin:
- Connecter à une API de prix réels (ex: CoinGecko pour crypto)
- Ajouter des documents joints (photos, factures)
- Historique des valeurs (tracking dans le temps)
- Partage sécurisé avec conseiller financier

---

## 🤝 Support

### Fichiers de référence:
1. `PATRIMOINE-EXTENSION.md` - Documentation extension
2. `test-assets-api.http` - Tests API
3. `BRIEF-TECHNIQUE-COMPLET.txt` - Documentation complète projet

### Structure du code:
- **Frontend**: `finance-ui/components/Asset*.tsx`
- **Types**: `finance-ui/types/asset.ts`
- **API Config**: `finance-ui/lib/api-config.ts`
- **Backend**: `FinanceApp/Controllers/AssetsController.cs`
- **Modèle**: `FinanceApp/Models/Asset.cs`

---

## ✅ Checklist de Validation

Avant de considérer la fonctionnalité comme opérationnelle:

- [ ] Backend démarre sans erreur (port 5152)
- [ ] Frontend démarre sans erreur (port 3000)
- [ ] PostgreSQL est actif (docker ps)
- [ ] Section "Mon Patrimoine" visible sur le dashboard
- [ ] Bouton "Ajouter un actif" fonctionne
- [ ] Modal s'ouvre et se ferme correctement
- [ ] Formulaire valide les champs requis
- [ ] POST /api/assets crée un nouvel actif
- [ ] La carte d'actif s'affiche avec le bon emoji
- [ ] Le patrimoine total se met à jour automatiquement
- [ ] Modification d'un actif fonctionne (icône ✏️)
- [ ] Suppression d'un actif fonctionne (icône 🗑️)
- [ ] Le gain/perte s'affiche si valeur d'achat fournie
- [ ] Format CAD correct (ex: 25 000,00 $)
- [ ] Aucune erreur TypeScript (`npm run build`)

---

**Status:** ✅ Implémentation complète  
**Version:** 1.0.0  
**Date:** 2 février 2025  

🎉 **Votre application est maintenant prête à gérer votre patrimoine !**
