import { useRouter } from 'next/navigation';
import { useEffect } from 'react';

export interface User {
  id: number;
  nom: string;
  email: string;
  createdAt: string;
}

/**
 * Hook personnalisé pour protéger les pages avec authentification
 * Redirige automatiquement vers /connexion si pas d'utilisateur authentifié
 */
export function useAuth() {
  const router = useRouter();

  const requireAuth = () => {
    // Vérifier l'authentification
    const userStr = sessionStorage.getItem('user');
    const token = sessionStorage.getItem('token');
    if (!userStr || !token) {
      // Rediriger vers connexion
      router.push('/connexion');
      return null;
    }

    try {
      const user: User = JSON.parse(userStr);
      return user;
    } catch {
      // stockage corrompu, nettoyer et rediriger
      sessionStorage.removeItem('user');
      sessionStorage.removeItem('token');
      router.push('/connexion');
      return null;
    }
  };

  const getUser = (): User | null => {
    try {
      const userStr = sessionStorage.getItem('user');
      if (!userStr) return null;
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  };

  const isAuthenticated = (): boolean => {
    try {
      const userStr = sessionStorage.getItem('user');
      const token = sessionStorage.getItem('token');
      return !!userStr && !!token && JSON.parse(userStr).id;
    } catch {
      return false;
    }
  };

  const logout = () => {
    sessionStorage.removeItem('user');
    sessionStorage.removeItem('token');
    router.push('/connexion');
  };

  return {
    requireAuth,
    getUser,
    isAuthenticated,
    logout,
  };
}
