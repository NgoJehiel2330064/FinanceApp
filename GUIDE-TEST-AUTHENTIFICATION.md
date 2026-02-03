# 🧪 Guide de Test - Authentification Sécurisée

## ✅ Checklist de Test

### Test 1: Accès à la Page d'Accueil (Non Authentifié)
- [ ] Ouvrir http://localhost:3000 dans un navigateur
- [ ] Vérifier la redirection automatique vers http://localhost:3000/connexion
- [ ] Vérifier la console (F12) pour les erreurs

**Résultat Attendu**: ✅ Redirection immédiate vers /connexion

---

### Test 2: Accès aux Pages Protégées (Non Authentifié)
- [ ] Essayer d'accéder à http://localhost:3000/transactions
- [ ] Essayer d'accéder à http://localhost:3000/statistiques
- [ ] Essayer d'accéder à http://localhost:3000/patrimoine

**Résultat Attendu**: ✅ Toutes les routes redirigent vers /connexion

---

### Test 3: Connexion et Accès à l'Accueil
- [ ] Aller à http://localhost:3000/connexion
- [ ] Se connecter avec un compte valide
- [ ] Vérifier que localStorage.user existe (F12 → Application → localStorage)
- [ ] Cliquer sur le lien "Tableau de Bord" ou aller à /
- [ ] Vérifier que la page se charge correctement

**Résultat Attendu**: ✅ Accès autorisé au tableau de bord

---

### Test 4: Navigation Entre Pages Protégées
- [ ] Depuis l'accueil, cliquer sur "Transactions"
- [ ] Vérifier le chargement de la page
- [ ] Cliquer sur "Statistiques"
- [ ] Cliquer sur "Patrimoine"
- [ ] Cliquer sur "Profil"

**Résultat Attendu**: ✅ Toutes les pages se chargent sans redirection

---

### Test 5: Suppression Manual du localStorage
- [ ] Ouvrir DevTools (F12)
- [ ] Aller à Application → localStorage → http://localhost:3000
- [ ] Cliquer droit sur "user" et "Delete"
- [ ] Rafraîchir la page (F5)

**Résultat Attendu**: ✅ Redirection vers /connexion

---

### Test 6: localStorage Corrompu
- [ ] Ouvrir DevTools (F12)
- [ ] Console → `localStorage.setItem('user', '{invalid json')`
- [ ] Rafraîchir la page

**Résultat Attendu**: ✅ Redirection vers /connexion + localStorage nettoyé

---

### Test 7: Logout
- [ ] Se connecter normalement
- [ ] Aller au /profil
- [ ] Cliquer sur "Déconnexion"
- [ ] Vérifier la redirection vers /connexion
- [ ] Vérifier que localStorage.user est supprimé

**Résultat Attendu**: ✅ Logout réussi + localStorage nettoyé

---

### Test 8: Pages Publiques (Inscription et Connexion)
- [ ] Accéder à http://localhost:3000/connexion sans authentification
- [ ] Vérifier qu'il y a accès (pas de redirection)
- [ ] Accéder à http://localhost:3000/inscription sans authentification
- [ ] Vérifier qu'il y a accès (pas de redirection)

**Résultat Attendu**: ✅ Accès direct sans redirection

---

### Test 9: Vérifier les Erreurs Console
- [ ] Ouvrir DevTools (F12)
- [ ] Aller à l'onglet "Console"
- [ ] Effectuer tous les tests ci-dessus
- [ ] Chercher les erreurs rouges (❌)

**Résultat Attendu**: ✅ Aucune erreur d'authentification

---

### Test 10: Performance du Loader
- [ ] (Optionnel) Ralentir la connexion réseau (DevTools → Network → Slow 3G)
- [ ] Rafraîchir une page protégée
- [ ] Vérifier que le loader s'affiche correctement

**Résultat Attendu**: ✅ Loader visible pendant la vérification

---

## 🔍 Vérifications Spécifiques

### localStorage.user Structure
```javascript
// Ouvrir Console (F12) et exécuter:
JSON.parse(localStorage.getItem('user'))
```

**Résultat Attendu:**
```json
{
  "id": 1,
  "nom": "Nom Utilisateur",
  "email": "user@example.com",
  "createdAt": "2025-01-15T..."
}
```

---

### Vérifier les Redirections
```javascript
// Console → Ajouter un breakpoint et vérifier:
router.push('/connexion') // Doit être appelé si pas d'user
```

---

## 📊 Résumé des Tests

| # | Test | Status |
|---|------|--------|
| 1 | Redirection /sans auth | ⏳ |
| 2 | Redirection routes | ⏳ |
| 3 | Connexion et accès | ⏳ |
| 4 | Navigation entre routes | ⏳ |
| 5 | Suppression localStorage | ⏳ |
| 6 | localStorage corrompu | ⏳ |
| 7 | Logout | ⏳ |
| 8 | Pages publiques | ⏳ |
| 9 | Console clean | ⏳ |
| 10 | Performance | ⏳ |

**Instructions**: Cocher les ✅ une fois testés

---

## 🐛 Troubleshooting

### Problème: Redirection en boucle
**Cause**: localStorage.user manquant ou corrompu
**Solution**: 
```javascript
localStorage.removeItem('user');
// Se reconnecter
```

### Problème: Page blanche
**Cause**: Erreur lors du parsing de localStorage.user
**Solution**:
1. Ouvrir DevTools (F12)
2. Vérifier Console pour les erreurs
3. Nettoyer localStorage
4. Rafraîchir

### Problème: Pas de redirection
**Cause**: Le hook useRouter n'est pas activé
**Solution**:
1. Vérifier que `'use client'` est en haut du fichier
2. Vérifier que `useRouter` est importé de `'next/navigation'`
3. Redémarrer le serveur dev

---

## 📝 Logs Utiles

### Activer le Debug
```typescript
// Dans any page:
useEffect(() => {
  console.log('[AUTH] Checking authentication...');
  const userStr = localStorage.getItem('user');
  console.log('[AUTH] User:', userStr ? JSON.parse(userStr) : 'null');
}, []);
```

---

## ✨ Test Complet (Scénario Réaliste)

```
1. Ouvrir Incognito (pas de localStorage)
2. Accéder à http://localhost:3000
3. ✅ Redirection vers /connexion
4. Remplir le formulaire et se connecter
5. ✅ Redirection vers / (accueil)
6. Cliquer sur "Transactions"
7. ✅ Chargement page transactions
8. Aller à profil et cliquer "Déconnexion"
9. ✅ Redirection vers /connexion
10. ✅ localStorage.user supprimé
```

---

## 🎯 Résultats Attendus Finaux

- ✅ Page d'accueil protégée
- ✅ Toutes les pages métier protégées
- ✅ Pages publiques accessibles
- ✅ Redirections automatiques
- ✅ localStorage géré correctement
- ✅ Logout fonctionnel
- ✅ Console sans erreurs

---

**Généré**: 2025-02-02  
**Statut**: 🟢 Test Ready
