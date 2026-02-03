'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { API_CONFIG, getApiUrl } from '@/lib/api-config';

interface DashboardStats {
  soldeNet: number;
  revenus: number;
  depenses: number;
  patrimoineTotal: number;
  transactionsCount: number;
  assetsCount: number;
}

export default function AccueilPage() {
  const [stats, setStats] = useState<DashboardStats>({
    soldeNet: 0,
    revenus: 0,
    depenses: 0,
    patrimoineTotal: 0,
    transactionsCount: 0,
    assetsCount: 0
  });
  const [loading, setLoading] = useState(true);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);

    const fetchStats = async () => {
      try {
        // Récupérer les transactions
        const transactionsRes = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.TRANSACTIONS));
        const transactions = transactionsRes.ok ? await transactionsRes.json() : [];

        // Récupérer les actifs
        const assetsRes = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.ASSETS));
        const assets = assetsRes.ok ? await assetsRes.json() : [];

        // Récupérer valeur totale patrimoine
        const totalRes = await fetch(getApiUrl(API_CONFIG.ENDPOINTS.ASSETS_TOTAL_VALUE));
        const totalData = totalRes.ok ? await totalRes.json() : { totalValue: 0 };

        // Calculer les stats
        const revenus = transactions
          .filter((t: { type: number }) => t.type === 1)
          .reduce((acc: number, t: { amount: number }) => acc + t.amount, 0);
        
        const depenses = transactions
          .filter((t: { type: number }) => t.type === 0)
          .reduce((acc: number, t: { amount: number }) => acc + Math.abs(t.amount), 0);

        setStats({
          soldeNet: revenus - depenses,
          revenus,
          depenses,
          patrimoineTotal: typeof totalData === 'number' ? totalData : totalData.totalValue || 0,
          transactionsCount: transactions.length,
          assetsCount: assets.length
        });
      } catch (err) {
        console.error('Erreur lors du chargement des stats:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, []);

  const formatMontant = (montant: number): string => {
    return new Intl.NumberFormat('fr-CA', {
      style: 'currency',
      currency: 'CAD'
    }).format(montant);
  };

  const getMotivationMessage = (): string => {
    const heure = new Date().getHours();
    if (heure >= 5 && heure < 12) return "☀️ Bonjour ! Commencez la journée du bon pied financier.";
    if (heure >= 12 && heure < 18) return "🌤️ Bon après-midi ! Votre argent travaille pour vous.";
    if (heure >= 18 && heure < 22) return "🌆 Bonsoir ! Prenez le contrôle de vos finances.";
    return "🌙 Bonne nuit ! Vos investissements dorment... pas vous ?";
  };

  return (
    <div className="min-h-screen bg-[#0f0f1e] text-white font-[family-name:var(--font-inter)]">
      {/* Cercles flous */}
      <div className="fixed top-0 left-0 w-full h-full overflow-hidden pointer-events-none">
        <div className="absolute top-[-20%] left-[-10%] w-[600px] h-[600px] bg-violet-600/30 rounded-full blur-[120px]"></div>
        <div className="absolute bottom-[-20%] right-[-10%] w-[700px] h-[700px] bg-blue-600/30 rounded-full blur-[120px]"></div>
      </div>

      <div className="relative z-10 max-w-7xl mx-auto px-6 py-12">
        {/* En-tête */}
        <header className={`mb-12 transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
          <h1 className="text-5xl font-bold mb-2 font-[family-name:var(--font-playfair)]">
            Tableau de Bord
          </h1>
          <p className="text-gray-400 text-lg">{getMotivationMessage()}</p>
        </header>

        {/* Chargement */}
        {loading ? (
          <div className="flex justify-center items-center py-20">
            <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-white/50"></div>
          </div>
        ) : (
          <>
            {/* Cartes de statistiques */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
              {/* Solde Net */}
              <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 animate-fadeIn" style={{ animationDelay: '0ms' }}>
                <p className="text-gray-400 text-sm mb-2">💰 Solde Net</p>
                <p className={`text-3xl font-bold font-[family-name:var(--font-playfair)] ${stats.soldeNet >= 0 ? 'text-emerald-400' : 'text-red-400'}`}>
                  {formatMontant(stats.soldeNet)}
                </p>
              </div>

              {/* Revenus */}
              <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 animate-fadeIn" style={{ animationDelay: '100ms' }}>
                <p className="text-gray-400 text-sm mb-2">📈 Revenus</p>
                <p className="text-3xl font-bold text-emerald-400 font-[family-name:var(--font-playfair)]">
                  {formatMontant(stats.revenus)}
                </p>
              </div>

              {/* Dépenses */}
              <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-6 animate-fadeIn" style={{ animationDelay: '200ms' }}>
                <p className="text-gray-400 text-sm mb-2">📉 Dépenses</p>
                <p className="text-3xl font-bold text-red-400 font-[family-name:var(--font-playfair)]">
                  {formatMontant(stats.depenses)}
                </p>
              </div>

              {/* Patrimoine */}
              <div className="backdrop-blur-xl bg-gradient-to-br from-purple-500/20 to-pink-500/20 border border-white/10 rounded-2xl p-6 animate-fadeIn" style={{ animationDelay: '300ms' }}>
                <p className="text-gray-400 text-sm mb-2">💎 Patrimoine Total</p>
                <p className="text-3xl font-bold text-white font-[family-name:var(--font-playfair)]">
                  {formatMontant(stats.patrimoineTotal)}
                </p>
              </div>
            </div>

            {/* Actions rapides */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Transactions */}
              <Link href="/transactions" className="group">
                <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl p-8 hover:bg-white/10 transition-all duration-300 animate-fadeIn" style={{ animationDelay: '400ms' }}>
                  <div className="flex items-start justify-between">
                    <div>
                      <div className="text-4xl mb-4">💳</div>
                      <h2 className="text-2xl font-bold text-white mb-2">Transactions</h2>
                      <p className="text-gray-400">
                        {stats.transactionsCount} transaction{stats.transactionsCount !== 1 ? 's' : ''} enregistrée{stats.transactionsCount !== 1 ? 's' : ''}
                      </p>
                    </div>
                    <svg className="w-6 h-6 text-gray-500 group-hover:text-white group-hover:translate-x-1 transition-all" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                  </div>
                </div>
              </Link>

              {/* Patrimoine */}
              <Link href="/patrimoine" className="group">
                <div className="backdrop-blur-xl bg-gradient-to-br from-indigo-500/10 to-purple-500/10 border border-white/10 rounded-2xl p-8 hover:from-indigo-500/20 hover:to-purple-500/20 transition-all duration-300 animate-fadeIn" style={{ animationDelay: '500ms' }}>
                  <div className="flex items-start justify-between">
                    <div>
                      <div className="text-4xl mb-4">💎</div>
                      <h2 className="text-2xl font-bold text-white mb-2">Patrimoine</h2>
                      <p className="text-gray-400">
                        {stats.assetsCount} actif{stats.assetsCount !== 1 ? 's' : ''} dans votre portefeuille
                      </p>
                    </div>
                    <svg className="w-6 h-6 text-gray-500 group-hover:text-white group-hover:translate-x-1 transition-all" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                  </div>
                </div>
              </Link>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
