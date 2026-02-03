# 🧪 Guide de Test - Isolation Multi-Utilisateur

## 🚀 Démarrage Rapide

### Prérequis
- Docker en cours d'exécution (PostgreSQL)
- Node.js installé (v18+)
- .NET 8 SDK installé

### Étape 1: Démarrer la Base de Données

```bash
# À la racine du projet
docker-compose up -d
```

Vérifier que PostgreSQL est actif sur `localhost:5432`

### Étape 2: Démarrer le Backend

```bash
cd "c:\Users\GOAT\OneDrive\Documents\FinanceApp\FinanceApp"
dotnet run
```

Devrait afficher:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
      Now listening on: https://localhost:5001
```

### Étape 3: Démarrer le Frontend

```bash
cd "c:\Users\GOAT\OneDrive\Documents\FinanceApp\finance-ui"
npm run dev
```

Devrait afficher:
```
▲ Next.js 16.1.6
✓ Ready in 2s
```

Naviguer vers: http://localhost:3000

---

## 🧪 Test 1: Isolation des Données

### Objectif
Vérifier que User A ne voit PAS les données de User B

### Étapes

#### 1️⃣ User A - Enregistrement et Login

1. Aller sur http://localhost:3000
2. Cliquer "Créer un compte"
3. Remplir le formulaire:
   - **Email**: `usera@test.com`
   - **Nom**: `Alice`
   - **Mot de passe**: `Test123!`
4. Cliquer "S'inscrire"
5. Page d'accueil devrait s'afficher

✅ **Vérification**: localStorage contient `{id: 1, email: "usera@test.com", nom: "Alice"}`

```javascript
// Dans la console du navigateur:
JSON.parse(localStorage.getItem('user'))
// Devrait retourner: {id: 1, email: "usera@test.com", nom: "Alice", createdAt: "..."}
```

#### 2️⃣ User A - Ajouter des Transactions

1. Aller sur "Transactions"
2. Cliquer "+ Ajouter une transaction"
3. Ajouter 3 transactions:

**Transaction 1:**
- Date: Aujourd'hui
- Description: "Épicerie Carrefour"
- Catégorie: "Alimentation"
- Type: "Dépense"
- Montant: 45.50
- ✅ Cliquer "Ajouter"

**Transaction 2:**
- Date: Hier
- Description: "Salaire Février"
- Catégorie: "Revenu"
- Type: "Revenu"
- Montant: 3500.00
- ✅ Cliquer "Ajouter"

**Transaction 3:**
- Date: 2 jours ago
- Description: "Essence Total"
- Catégorie: "Transport"
- Type: "Dépense"
- Montant: 60.00
- ✅ Cliquer "Ajouter"

✅ **Vérification**: Voir les 3 transactions listées

#### 3️⃣ User A - Vérifier dans la Base de Données

```bash
# Se connecter à PostgreSQL
psql -U postgres -h localhost -d finance_db

# Vérifier les données
SELECT id, "UserId", description, amount FROM "Transactions";
```

Résultat attendu:
```
 id | UserId |     description     | amount
----+--------+---------------------+--------
  1 |      1 | Épicerie Carrefour  | -45.50
  2 |      1 | Salaire Février     | 3500.00
  3 |      1 | Essence Total       | -60.00
```

✅ Toutes les transactions ont `UserId = 1`

#### 4️⃣ User A - Se Déconnecter

1. Cliquer sur le menu profil (en haut à droite)
2. Cliquer "Se déconnecter"
3. Devrait être redirigé vers `/connexion`

#### 5️⃣ User B - Enregistrement et Login

1. Aller sur http://localhost:3000/connexion
2. Cliquer "Créer un compte"
3. Remplir le formulaire:
   - **Email**: `userb@test.com`
   - **Nom**: `Bob`
   - **Mot de passe**: `Test123!`
4. Cliquer "S'inscrire"
5. Page d'accueil devrait s'afficher

✅ **Vérification**: `{id: 2, email: "userb@test.com", nom: "Bob"}`

#### 6️⃣ User B - Ajouter des Transactions

1. Aller sur "Transactions"
2. Ajouter 2 transactions:

**Transaction 1:**
- Description: "Pizza Restaurant"
- Montant: 25.00 (Dépense)

**Transaction 2:**
- Description: "Freelance Travail"
- Montant: 500.00 (Revenu)

✅ **Vérification**: Voir 2 transactions (PAS les 3 de User A!)

#### 7️⃣ Vérifier les Données en Base

```bash
SELECT id, "UserId", description, amount FROM "Transactions" ORDER BY id;
```

Résultat attendu:
```
 id | UserId |       description       | amount
----+--------+-------------------------+--------
  1 |      1 | Épicerie Carrefour      | -45.50
  2 |      1 | Salaire Février         | 3500.00
  3 |      1 | Essence Total           | -60.00
  4 |      2 | Pizza Restaurant        | -25.00
  5 |      2 | Freelance Travail       | 500.00
```

✅ Transactions 4 et 5 ont `UserId = 2`

#### 8️⃣ User B - Vérifier l'Isolation

1. Aller sur "Transactions"
2. **Vérification critique**: Devrait voir **UNIQUEMENT** les 2 transactions de User B
3. **NE PAS voir** les 3 transactions de User A

❌ **Si voir les 5 transactions** → Isolation échouée!
✅ **Si voir seulement les 2** → Isolation réussie! 🎉

#### 9️⃣ User B - Se Reconnecter en User A

1. Se déconnecter
2. Aller à `/connexion`
3. Login avec User A (usera@test.com / Test123!)
4. Aller sur "Transactions"
5. **Vérification**: Devrait voir **UNIQUEMENT** les 3 transactions de User A

✅ **ISOLATION CONFIRMÉE!**

---

## 🧪 Test 2: Tentative d'Accès Non-Autorisé

### Objectif
Vérifier que User B ne peut pas accéder/modifier les données de User A

### Étapes

#### 1️⃣ User B - Utiliser l'API Directement

1. Ouvrir la console du navigateur (F12)
2. User B doit être connecté (userId = 2)
3. Exécuter dans la console:

```javascript
// Essayer de récupérer les transactions de User A
fetch('http://localhost:5000/api/transactions?userId=1')
    .then(r => r.json())
    .then(d => console.log(d));
```

❌ **Résultat attendu**: Tableau vide `[]`
✅ **Correct**: User B ne peut pas voir les transactions de User A

#### 2️⃣ User B - Tentative de Modification

```javascript
// Essayer de modifier la transaction 1 (de User A)
fetch('http://localhost:5000/api/transactions/1?userId=2', {
    method: 'PUT',
    headers: {'Content-Type': 'application/json'},
    body: JSON.stringify({amount: 9999, description: "HACKER"})
})
.then(r => ({status: r.status, ok: r.ok}))
.then(d => console.log(d));
```

❌ **Résultat attendu**: Status 403 (Forbidden)
✅ **Correct**: User B n'a pas accès à la modification

#### 3️⃣ Vérifier que Transaction n'a pas changé

```bash
psql -U postgres -h localhost -d finance_db
SELECT * FROM "Transactions" WHERE id = 1;
```

✅ **Vérification**: Amount devrait toujours être `-45.50` (pas modifié)

---

## 🧪 Test 3: Actifs/Patrimoine

### Objectif
Vérifier l'isolation des actifs entre utilisateurs

### Étapes

#### 1️⃣ User A - Ajouter des Actifs

1. Login en tant que User A
2. Aller sur "Patrimoine"
3. Cliquer "+ Ajouter un actif"
4. Ajouter 3 actifs:

**Actif 1: Compte Bancaire**
- Nom: "Compte Courant BNP"
- Type: "Compte Bancaire"
- Valeur: 15432.50 CAD

**Actif 2: Maison**
- Nom: "Appartement Paris 12e"
- Type: "Immobilier"
- Valeur: 500000.00 CAD

**Actif 3: Voiture**
- Nom: "Renault Scenic 2020"
- Type: "Véhicule"
- Valeur: 25000.00 CAD

✅ **Vérification**: Total patrimoine = 540,432.50 CAD

#### 2️⃣ User A - Vérifier les Données

```bash
SELECT "UserId", name, "CurrentValue" FROM "Assets" ORDER BY id;
```

Résultat:
```
 UserId |              name              | CurrentValue
--------+--------------------------------+--------------
      1 | Compte Courant BNP             |   15432.50
      1 | Appartement Paris 12e          |  500000.00
      1 | Renault Scenic 2020            |   25000.00
```

✅ Tous avec `UserId = 1`

#### 3️⃣ User B - Se Connecter

1. Se déconnecter en User A
2. Login en tant que User B

#### 4️⃣ User B - Vérifier le Patrimoine Vide

1. Aller sur "Patrimoine"
2. **Vérification critique**: Devrait être vide (0 actifs, 0 CAD)

#### 5️⃣ User B - Ajouter ses Propres Actifs

1. Ajouter 1 actif:
   - Nom: "Compte Éco Banque Postale"
   - Type: "Compte Bancaire"
   - Valeur: 8500.00 CAD

✅ **Vérification**: Total patrimoine = 8,500.00 CAD

#### 6️⃣ User B - Vérifier l'Isolation

```bash
SELECT "UserId", name, "CurrentValue" FROM "Assets" ORDER BY id;
```

Résultat:
```
 UserId |              name              | CurrentValue
--------+--------------------------------+--------------
      1 | Compte Courant BNP             |   15432.50
      1 | Appartement Paris 12e          |  500000.00
      1 | Renault Scenic 2020            |   25000.00
      2 | Compte Éco Banque Postale      |    8500.00
```

✅ User A a 3 actifs, User B a 1 actif

#### 7️⃣ User A - Reconnecter et Vérifier

1. Se déconnecter en User B
2. Login en tant que User A
3. Aller sur "Patrimoine"
4. **Vérification**: Devrait voir ses 3 actifs (540,432.50 CAD)
5. **Devrait NE PAS voir** l'actif de User B

✅ **ISOLATION DES ACTIFS CONFIRMÉE!**

---

## 🧪 Test 4: Vérification des API Directs

### Vérifier avec curl

```bash
# User A getting their transactions
curl "http://localhost:5000/api/transactions?userId=1"
# Retour: Array avec 3 transactions

# User B trying to access User A's data
curl "http://localhost:5000/api/transactions?userId=1" -H "Authorization: Bearer UserB"
# Retour: Array vide [] (l'API ne sait pas qui est l'utilisateur, donc userId=1 en query)

# User B getting their own transactions
curl "http://localhost:5000/api/transactions?userId=2"
# Retour: Array avec 2 transactions

# User B trying to delete User A's transaction
curl -X DELETE "http://localhost:5000/api/transactions/1?userId=2"
# Retour: 403 Forbidden (pas propriétaire)
```

---

## ✅ Checklist de Tests Réussis

- [ ] User A peut créer 3 transactions
- [ ] User B ne voit pas les transactions de User A
- [ ] User B peut créer 2 transactions
- [ ] User A ne voit pas les transactions de User B
- [ ] User B ne peut pas modifier les transactions de User A (403 Forbidden)
- [ ] Base de données montre bien les UserId séparés
- [ ] User A peut créer actifs
- [ ] User B ne voit pas les actifs de User A
- [ ] Les totaux de patrimoine sont corrects par utilisateur
- [ ] API retourne 403 pour accès non-autorisé

---

## 🐛 Debugging

Si tests échouent:

### Transaction vue par tous les utilisateurs
```bash
# Vérifier que le frontend passe userId
# Ouvrir DevTools → Network → transactions
# Chercher ?userId=XXX dans l'URL

# Vérifier que le backend filtre
# Logs backend doivent montrer:
# "Demande de récupération des transactions pour l'utilisateur X"
```

### 403 Forbidden ne s'affiche pas
```bash
# Vérifier le contrôleur
# La propriété UserId doit être vérifiée avant PUT/DELETE
if (transaction.UserId != userId)
    return Forbid();
```

### Base de données vide
```bash
# S'assurer que la migration a été appliquée
dotnet ef database update

# Vérifier les colonnes UserId existent
\d "Transactions"
\d "Assets"
```

---

## 📝 Notes

- **localhost:3000** = Frontend
- **localhost:5000** = Backend HTTP
- **localhost:5001** = Backend HTTPS
- **localhost:5432** = PostgreSQL (Docker)

- userId = 1 = User A (usera@test.com)
- userId = 2 = User B (userb@test.com)

---

## 🎯 Résultat Attendu

✅ **Si tous les tests passent**:
- Isolation multi-utilisateur CONFIRMÉE
- Application SÉCURISÉE pour production
- Prêt pour déploiement

❌ **Si un test échoue**:
- Revoir les changements dans le fichier concerné
- Vérifier les logs (DevTools ou console)
- Relancer le backend et frontend
