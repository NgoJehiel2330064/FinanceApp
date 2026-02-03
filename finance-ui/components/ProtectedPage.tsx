/**
 * Composant wrapper pour les pages protégées
 * Gère la redirection vers /connexion si pas authentifié
 */
'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';

interface ProtectedPageProps {
  children: React.ReactNode;
  showLoader?: boolean;
}

export function ProtectedPage({ children, showLoader = true }: ProtectedPageProps) {
  const router = useRouter();
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Vérifier l'authentification
    const userStr = sessionStorage.getItem('user');
    const token = sessionStorage.getItem('token');
    
    if (!userStr || !token) {
      // Pas d'utilisateur, rediriger vers connexion
      router.push('/connexion');
      return;
    }

    try {
      JSON.parse(userStr);
      setIsAuthorized(true);
    } catch {
      // stockage corrompu
      sessionStorage.removeItem('user');
      sessionStorage.removeItem('token');
      router.push('/connexion');
    } finally {
      setIsLoading(false);
    }
  }, [router]);

  if (!isAuthorized) {
    if (showLoader && isLoading) {
      return (
        <div className="min-h-screen bg-[#0f0f1e] flex items-center justify-center">
          <div className="text-white">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-white"></div>
            <p className="mt-4">Vérification de l'authentification...</p>
          </div>
        </div>
      );
    }
    return null;
  }

  return <>{children}</>;
}
