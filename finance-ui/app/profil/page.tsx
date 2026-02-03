'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { API_CONFIG, getApiUrl } from '@/lib/api-config';
import { clearUserCookie, getAuthHeaders } from '@/lib/cookie-utils';

interface UserProfile {
  id: number;
  nom: string;
  email: string;
  createdAt: string;
}

export default function ProfilePage() {
  const router = useRouter();
  const [user, setUser] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [mounted, setMounted] = useState<boolean>(false);

  // Modales
  const [showChangePasswordModal, setShowChangePasswordModal] = useState<boolean>(false);
  const [showDeleteAccountModal, setShowDeleteAccountModal] = useState<boolean>(false);

  // États du formulaire changement de mot de passe
  const [changePasswordData, setChangePasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [changingPassword, setChangingPassword] = useState<boolean>(false);
  const [passwordChangeSuccess, setPasswordChangeSuccess] = useState<boolean>(false);

  // États de suppression de compte
  const [deletePassword, setDeletePassword] = useState<string>('');
  const [deletingAccount, setDeletingAccount] = useState<boolean>(false);

  // Récupération du profil utilisateur
  useEffect(() => {
    setMounted(true);

    const fetchProfile = async () => {
      try {
        setLoading(true);

        // Récupérer l'ID utilisateur du sessionStorage
        const userStr = sessionStorage.getItem('user');
        if (!userStr) {
          // Rediriger vers connexion
          router.push('/connexion');
          return;
        }

        const userData = JSON.parse(userStr);
        const userId = userData.id;

        // Récupérer le profil complet
        const response = await fetch(
          `${getApiUrl(API_CONFIG.ENDPOINTS.GET_PROFILE)}?userId=${userId}`,
          { headers: getAuthHeaders() }
        );

        if (!response.ok) {
          const errorData = await response.json().catch(() => ({}));
          console.error('Erreur profil:', response.status, errorData);
          throw new Error(errorData.message || `Erreur ${response.status} lors du chargement du profil`);
        }

        const data = await response.json();
        setUser(data.user);
        setError(null);
      } catch (err) {
        console.error('Erreur:', err);
        setError('Impossible de charger le profil.');
      } finally {
        setLoading(false);
      }
    };

    fetchProfile();
  }, [router]);

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('fr-FR', {
      day: '2-digit',
      month: 'long',
      year: 'numeric'
    });
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();

    if (changePasswordData.newPassword !== changePasswordData.confirmPassword) {
      alert('Les mots de passe ne correspondent pas');
      return;
    }

    if (changePasswordData.newPassword.length < 8) {
      alert('Le nouveau mot de passe doit contenir au moins 8 caractères');
      return;
    }

    if (!user) return;

    setChangingPassword(true);
    try {
      const response = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.CHANGE_PASSWORD), {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({
          userId: user.id,
          currentPassword: changePasswordData.currentPassword,
          newPassword: changePasswordData.newPassword
        })
      });

      if (!response.ok) {
        const data = await response.json();
        throw new Error(data.message || 'Erreur lors du changement de mot de passe');
      }

      setPasswordChangeSuccess(true);
      setChangePasswordData({ currentPassword: '', newPassword: '', confirmPassword: '' });
      
      setTimeout(() => {
        setShowChangePasswordModal(false);
        setPasswordChangeSuccess(false);
      }, 2000);
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Erreur lors du changement de mot de passe');
    } finally {
      setChangingPassword(false);
    }
  };

  const handleDeleteAccount = async () => {
    if (!user) return;

    if (deletePassword.trim() === '') {
      alert('Veuillez entrer votre mot de passe');
      return;
    }

    setDeletingAccount(true);
    try {
      const response = await fetch(
        `${getApiUrl(API_CONFIG.ENDPOINTS.DELETE_ACCOUNT)}?userId=${user.id}&password=${encodeURIComponent(deletePassword)}`,
        {
          method: 'DELETE',
          headers: getAuthHeaders()
        }
      );

      if (!response.ok) {
        const data = await response.json();
        throw new Error(data.message || 'Erreur lors de la suppression');
      }

      // Déconnexion et redirection
      clearUserCookie();
      router.push('/connexion');
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Erreur lors de la suppression du compte');
    } finally {
      setDeletingAccount(false);
    }
  };

  const handleLogout = () => {
    clearUserCookie();
    router.push('/connexion');
  };

  return (
    <div className="min-h-screen bg-[#0f0f1e] text-white font-[family-name:var(--font-inter)]">
      {/* Cercles flous */}
      <div className="fixed top-0 left-0 w-full h-full overflow-hidden pointer-events-none">
        <div className="absolute top-[-20%] left-[-10%] w-[600px] h-[600px] bg-violet-600/30 rounded-full blur-[120px]"></div>
        <div className="absolute bottom-[-20%] right-[-10%] w-[700px] h-[700px] bg-blue-600/30 rounded-full blur-[120px]"></div>
      </div>

      <div className="relative z-10 max-w-2xl mx-auto px-6 py-12">
        {/* En-tête */}
        <header className={`mb-12 transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
          <h1 className="text-5xl font-bold mb-2 font-[family-name:var(--font-playfair)]">
            👤 Mon Profil
          </h1>
          <p className="text-gray-400 text-lg">Gérez votre compte et vos paramètres</p>
        </header>

        {/* Loading */}
        {loading && (
          <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-16 w-16 border-t-2 border-b-2 border-violet-500"></div>
          </div>
        )}

        {/* Erreur */}
        {error && (
          <div className="backdrop-blur-xl bg-red-500/10 border border-red-500/30 rounded-2xl p-6 mb-8">
            <p className="text-red-400">❌ Erreur : {error}</p>
          </div>
        )}

        {/* Contenu principal */}
        {!loading && user && (
          <>
            {/* Informations du compte */}
            <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-8 mb-8">
              <h2 className="text-2xl font-bold mb-6 font-[family-name:var(--font-playfair)]">
                📋 Informations du Compte
              </h2>

              <div className="space-y-6">
                {/* Nom */}
                <div>
                  <label className="text-sm font-medium text-gray-400 uppercase tracking-wider">Nom</label>
                  <p className="text-2xl font-bold text-white mt-2">{user.nom}</p>
                </div>

                {/* Email */}
                <div>
                  <label className="text-sm font-medium text-gray-400 uppercase tracking-wider">Email</label>
                  <p className="text-lg text-white mt-2">{user.email}</p>
                </div>

                {/* Date d'inscription */}
                <div>
                  <label className="text-sm font-medium text-gray-400 uppercase tracking-wider">Membre depuis</label>
                  <p className="text-lg text-white mt-2">
                    {formatDate(user.createdAt)}
                  </p>
                </div>
              </div>
            </div>

            {/* Actions */}
            <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-8 mb-8">
              <h2 className="text-2xl font-bold mb-6 font-[family-name:var(--font-playfair)]">
                ⚙️ Actions
              </h2>

              <div className="space-y-4">
                {/* Changement de mot de passe */}
                <button
                  onClick={() => setShowChangePasswordModal(true)}
                  className="w-full backdrop-blur-xl bg-blue-500/20 hover:bg-blue-500/30 border border-blue-500/30 rounded-xl px-6 py-4 text-left transition-all text-white font-medium"
                >
                  <div className="flex items-center justify-between">
                    <span>🔐 Changer le mot de passe</span>
                    <span className="text-gray-400">→</span>
                  </div>
                </button>

                {/* Déconnexion */}
                <button
                  onClick={handleLogout}
                  className="w-full backdrop-blur-xl bg-amber-500/20 hover:bg-amber-500/30 border border-amber-500/30 rounded-xl px-6 py-4 text-left transition-all text-white font-medium"
                >
                  <div className="flex items-center justify-between">
                    <span>🚪 Déconnexion</span>
                    <span className="text-gray-400">→</span>
                  </div>
                </button>

                {/* Suppression de compte */}
                <button
                  onClick={() => setShowDeleteAccountModal(true)}
                  className="w-full backdrop-blur-xl bg-red-500/20 hover:bg-red-500/30 border border-red-500/30 rounded-xl px-6 py-4 text-left transition-all text-white font-medium"
                >
                  <div className="flex items-center justify-between">
                    <span>🗑️ Supprimer le compte</span>
                    <span className="text-gray-400">→</span>
                  </div>
                </button>
              </div>
            </div>
          </>
        )}
      </div>

      {/* Modal Changement de mot de passe */}
      {showChangePasswordModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 animate-fadeIn">
          <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={() => !changingPassword && setShowChangePasswordModal(false)}></div>

          <div className="relative backdrop-blur-xl bg-white/10 border border-white/20 rounded-3xl p-8 max-w-md w-full shadow-2xl animate-scaleIn">
            <button onClick={() => !changingPassword && setShowChangePasswordModal(false)} className="absolute top-4 right-4 text-gray-400 hover:text-white transition-colors">
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>

            <h2 className="text-3xl font-bold mb-6 font-[family-name:var(--font-playfair)]">
              🔐 Changer le mot de passe
            </h2>

            {passwordChangeSuccess ? (
              <div className="text-center py-8">
                <p className="text-2xl mb-2">✅</p>
                <p className="text-white font-semibold">Mot de passe changé avec succès !</p>
              </div>
            ) : (
              <form onSubmit={handleChangePassword} className="space-y-5">
                {/* Mot de passe actuel */}
                <div>
                  <label className="block text-sm font-medium mb-2 text-gray-300">Mot de passe actuel</label>
                  <input
                    type="password"
                    required
                    value={changePasswordData.currentPassword}
                    onChange={(e) => setChangePasswordData({...changePasswordData, currentPassword: e.target.value})}
                    className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white placeholder-gray-500 focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                    placeholder="Entrez votre mot de passe actuel"
                    disabled={changingPassword}
                  />
                </div>

                {/* Nouveau mot de passe */}
                <div>
                  <label className="block text-sm font-medium mb-2 text-gray-300">Nouveau mot de passe</label>
                  <input
                    type="password"
                    required
                    minLength={8}
                    value={changePasswordData.newPassword}
                    onChange={(e) => setChangePasswordData({...changePasswordData, newPassword: e.target.value})}
                    className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white placeholder-gray-500 focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                    placeholder="Min. 8 caractères"
                    disabled={changingPassword}
                  />
                </div>

                {/* Confirmer mot de passe */}
                <div>
                  <label className="block text-sm font-medium mb-2 text-gray-300">Confirmer le mot de passe</label>
                  <input
                    type="password"
                    required
                    minLength={8}
                    value={changePasswordData.confirmPassword}
                    onChange={(e) => setChangePasswordData({...changePasswordData, confirmPassword: e.target.value})}
                    className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white placeholder-gray-500 focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/50 transition-all"
                    placeholder="Confirmez le nouveau mot de passe"
                    disabled={changingPassword}
                  />
                </div>

                {/* Boutons */}
                <div className="flex gap-4 pt-4">
                  <button
                    type="button"
                    onClick={() => setShowChangePasswordModal(false)}
                    disabled={changingPassword}
                    className="flex-1 bg-white/5 hover:bg-white/10 text-white font-semibold py-3 rounded-xl transition-all disabled:opacity-50"
                  >
                    Annuler
                  </button>
                  <button
                    type="submit"
                    disabled={changingPassword}
                    className="flex-1 bg-gradient-to-r from-blue-600 to-cyan-600 hover:from-blue-700 hover:to-cyan-700 text-white font-semibold py-3 rounded-xl shadow-lg shadow-blue-500/50 transition-all hover:scale-105 disabled:opacity-50"
                  >
                    {changingPassword ? 'Changement...' : 'Changer'}
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>
      )}

      {/* Modal Suppression de compte */}
      {showDeleteAccountModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 animate-fadeIn">
          <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={() => !deletingAccount && setShowDeleteAccountModal(false)}></div>

          <div className="relative backdrop-blur-xl bg-white/10 border border-white/20 rounded-3xl p-8 max-w-md w-full shadow-2xl animate-scaleIn">
            <button onClick={() => !deletingAccount && setShowDeleteAccountModal(false)} className="absolute top-4 right-4 text-gray-400 hover:text-white transition-colors" disabled={deletingAccount}>
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>

            <h2 className="text-3xl font-bold mb-4 font-[family-name:var(--font-playfair)] text-red-400">
              ⚠️ Supprimer le compte
            </h2>

            <p className="text-gray-300 mb-6 leading-relaxed">
              Cette action est <span className="text-red-400 font-bold">irréversible</span>. Votre compte et toutes vos données seront supprimés définitivement.
            </p>

            {/* Mot de passe */}
            <div className="mb-6">
              <label className="block text-sm font-medium mb-2 text-gray-300">Confirmer avec votre mot de passe</label>
              <input
                type="password"
                value={deletePassword}
                onChange={(e) => setDeletePassword(e.target.value)}
                className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white placeholder-gray-500 focus:outline-none focus:border-red-500 focus:ring-2 focus:ring-red-500/50 transition-all"
                placeholder="Entrez votre mot de passe"
                disabled={deletingAccount}
              />
            </div>

            {/* Boutons */}
            <div className="flex gap-4">
              <button
                onClick={() => setShowDeleteAccountModal(false)}
                disabled={deletingAccount}
                className="flex-1 bg-white/5 hover:bg-white/10 text-white font-semibold py-3 rounded-xl transition-all disabled:opacity-50"
              >
                Annuler
              </button>
              <button
                onClick={handleDeleteAccount}
                disabled={deletingAccount || deletePassword.trim() === ''}
                className="flex-1 bg-red-600 hover:bg-red-700 text-white font-semibold py-3 rounded-xl shadow-lg shadow-red-500/50 transition-all hover:scale-105 disabled:opacity-50"
              >
                {deletingAccount ? 'Suppression...' : 'Supprimer'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
