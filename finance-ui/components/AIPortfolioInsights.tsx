'use client';

import { useEffect, useState } from 'react';
import { API_CONFIG, getApiUrl } from '@/lib/api-config';
import { getAuthHeaders } from '@/lib/cookie-utils';

interface AIPortfolioInsightsProps {
  assetCount: number;
}

export default function AIPortfolioInsights({ assetCount }: AIPortfolioInsightsProps) {
  const [insights, setInsights] = useState<string[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Ne charger que si des actifs existent
    if (assetCount === 0) {
      setLoading(false);
      return;
    }

    const fetchInsights = async () => {
      try {
        setLoading(true);
        setError(null);

        const userStr = sessionStorage.getItem('user');
        if (!userStr) {
          throw new Error('Utilisateur non connecté');
        }

        const user = JSON.parse(userStr);
        const userId = user.id;

        const response = await fetch(
          `${getApiUrl(API_CONFIG.ENDPOINTS.PORTFOLIO_INSIGHTS)}?userId=${userId}`,
          { headers: getAuthHeaders() }
        );

        if (!response.ok) {
          throw new Error(`Erreur HTTP ${response.status}`);
        }

        const data: { insights: string[] } = await response.json();
        setInsights(data.insights || []);
      } catch (err) {
        console.error('Erreur lors de la récupération des insights:', err);
        setError(err instanceof Error ? err.message : 'Erreur lors du chargement des insights');
      } finally {
        setLoading(false);
      }
    };

    fetchInsights();
  }, [assetCount]);

  // Ne rien afficher si pas d'actifs
  if (assetCount === 0) {
    return null;
  }

  // État de chargement
  if (loading) {
    return (
      <div className="backdrop-blur-xl bg-gradient-to-br from-indigo-500/20 to-purple-500/20 border border-white/10 rounded-2xl p-8 animate-fadeIn">
        <div className="flex items-center gap-4">
          <div className="animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-white/50"></div>
          <p className="text-gray-300">Génération des insights IA...</p>
        </div>
      </div>
    );
  }

  // État d'erreur
  if (error) {
    return (
      <div className="backdrop-blur-xl bg-red-500/10 border border-red-500/20 rounded-2xl p-6 animate-fadeIn">
        <div className="flex items-start gap-3">
          <svg className="w-6 h-6 text-red-400 flex-shrink-0 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div>
            <p className="text-red-300 font-medium">Insights IA temporairement indisponibles</p>
            <p className="text-red-400/80 text-sm mt-1">{error}</p>
          </div>
        </div>
      </div>
    );
  }

  // Aucun insight disponible
  if (insights.length === 0) {
    return null;
  }

  // Affichage des insights
  return (
    <div className="backdrop-blur-xl bg-gradient-to-br from-indigo-500/20 to-purple-500/20 border border-white/10 rounded-2xl p-8 animate-fadeIn">
      <div className="flex items-start gap-4 mb-6">
        <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-indigo-500 to-purple-500 flex items-center justify-center flex-shrink-0 animate-shimmer">
          <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
          </svg>
        </div>
        <div>
          <h3 className="text-2xl font-bold text-white font-[family-name:var(--font-playfair)]">
            Analyse Patrimoniale IA
          </h3>
          <p className="text-gray-400 text-sm mt-1">Recommandations personnalisées basées sur votre portefeuille</p>
        </div>
      </div>

      <div className="space-y-3">
        {insights.map((insight, index) => (
          <div
            key={index}
            className="flex gap-3 p-4 rounded-xl bg-white/5 hover:bg-white/10 transition-all duration-300 animate-fadeIn"
            style={{ animationDelay: `${index * 100}ms` }}
          >
            <span className="text-indigo-400 font-bold flex-shrink-0 mt-0.5">•</span>
            <p className="text-gray-200 leading-relaxed">{insight}</p>
          </div>
        ))}
      </div>

      <div className="mt-6 pt-6 border-t border-white/10">
        <p className="text-gray-400 text-xs text-center">
          💡 Insights générés par Google Gemini AI
        </p>
      </div>
    </div>
  );
}
