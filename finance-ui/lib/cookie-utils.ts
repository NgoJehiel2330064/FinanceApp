/**
 * Utilitaires pour gérer les cookies d'authentification
 */

/**
 * Définir la session utilisateur après connexion (JWT + sessionStorage)
 */
export function setUserCookie(user: any, token?: string) {
  if (token) {
    // Cookie de session (expire à la fermeture du navigateur)
    document.cookie = `auth_token=${encodeURIComponent(token)}; path=/; SameSite=Strict`;
    sessionStorage.setItem('token', token);
  }

  // Stocker l'utilisateur en sessionStorage
  sessionStorage.setItem('user', JSON.stringify(user));
}

/**
 * Supprimer le cookie utilisateur à la déconnexion
 */
export function clearUserCookie() {
  // Supprimer le cookie en définissant une date d'expiration passée
  document.cookie = 'auth_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 UTC; SameSite=Strict';

  // Vider la session
  sessionStorage.removeItem('user');
  sessionStorage.removeItem('token');
}

/**
 * Récupérer l'utilisateur depuis sessionStorage (accessible client-side)
 */
export function getUserFromStorage() {
  if (typeof window === 'undefined') return null;
  
  const userStr = sessionStorage.getItem('user');
  if (!userStr) return null;
  
  try {
    return JSON.parse(userStr);
  } catch {
    return null;
  }
}

/**
 * Vérifier si l'utilisateur est connecté
 */
export function isUserAuthenticated(): boolean {
  if (typeof window === 'undefined') return false;
  return !!sessionStorage.getItem('token') && !!sessionStorage.getItem('user');
}

/**
 * Récupérer le token JWT depuis la session
 */
export function getAuthToken(): string | null {
  if (typeof window === 'undefined') return null;
  return sessionStorage.getItem('token');
}

/**
 * Générer les headers d'authentification
 */
export function getAuthHeaders(): HeadersInit {
  const token = getAuthToken();

  if (!token) {
    return {
      'Content-Type': 'application/json',
    };
  }

  return {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token}`,
  };
}
