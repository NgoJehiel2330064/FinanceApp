'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { API_CONFIG, getApiUrl } from '@/lib/api-config';
import { getAuthHeaders } from '@/lib/cookie-utils';
import { Asset, AssetFormData } from '@/types/asset';
import { Liability, LiabilityFormData } from '@/types/liability';
import { liabilityService, netWorthService } from '@/lib/liability-service';
import AssetList from '@/components/AssetList';
import AssetModal from '@/components/AssetModal';
import LiabilityCard from '@/components/LiabilityCard';
import LiabilityModal from '@/components/LiabilityModal';
import AIPortfolioInsights from '@/components/AIPortfolioInsights';

export default function PatrimoinePage() {
  const router = useRouter();
  
  // STATE - ACTIFS
  const [assets, setAssets] = useState<Asset[]>([]);
  const [totalAssetValue, setTotalAssetValue] = useState<number>(0);
  const [assetsLoading, setAssetsLoading] = useState<boolean>(true);
  const [assetsError, setAssetsError] = useState<string | null>(null);
  const [showAssetModal, setShowAssetModal] = useState<boolean>(false);
  const [editingAsset, setEditingAsset] = useState<Asset | null>(null);

  // STATE - PASSIFS (DETTES)
  const [liabilities, setLiabilities] = useState<Liability[]>([]);
  const [totalDebt, setTotalDebt] = useState<number>(0);
  const [liabilitiesLoading, setLiabilitiesLoading] = useState<boolean>(true);
  const [showLiabilityModal, setShowLiabilityModal] = useState<boolean>(false);
  const [editingLiability, setEditingLiability] = useState<Liability | null>(null);

  // STATE - PATRIMOINE NET
  const [netWorth, setNetWorth] = useState<number>(0);
  const [transactionBalance, setTransactionBalance] = useState<number>(0);
  const [creditUtilization, setCreditUtilization] = useState<number>(0);

  const [mounted, setMounted] = useState<boolean>(false);
  const [activeTab, setActiveTab] = useState<'actifs' | 'dettes'>('actifs');

  useEffect(() => {
    setMounted(true);

    const userStr = sessionStorage.getItem('user');
    if (!userStr) {
      router.push('/connexion');
      return;
    }

    fetchAllData();
  }, []);

  const fetchAllData = async () => {
    try {
      const userStr = sessionStorage.getItem('user');
      if (!userStr) return;
      
      const user = JSON.parse(userStr);
      const userId = user.id;

      // Récupérer actifs
      setAssetsLoading(true);
      const assetsResponse = await fetch(
        `${getApiUrl(API_CONFIG.ENDPOINTS.ASSETS)}?userId=${userId}`,
        { headers: getAuthHeaders() }
      );
      if (assetsResponse.ok) {
        const assetsData: Asset[] = await assetsResponse.json();
        setAssets(assetsData);
        setTotalAssetValue(assetsData.reduce((sum, a) => sum + a.currentValue, 0));
      }

      // Récupérer passifs
      setLiabilitiesLoading(true);
      const liabilitiesData = await liabilityService.getAll(userId);
      setLiabilities(liabilitiesData);
      setTotalDebt(liabilitiesData.reduce((sum, l) => sum + l.currentBalance, 0));

      // Récupérer patrimoine net complet
      const netWorthData = await netWorthService.getNetWorth(userId);
      setNetWorth(netWorthData.netWorth);
      setTransactionBalance(netWorthData.transactionBalance);
      setCreditUtilization(netWorthData.creditUtilization);

    } catch (err) {
      console.error('Erreur lors de la récupération des données:', err);
      setAssetsError(err instanceof Error ? err.message : 'Erreur de chargement');
    } finally {
      setAssetsLoading(false);
      setLiabilitiesLoading(false);
    }
  };

  // HANDLERS ACTIFS
  const handleAssetSubmit = async (assetData: AssetFormData) => {
    try {
      const userStr = sessionStorage.getItem('user');
      if (!userStr) return;
      
      const user = JSON.parse(userStr);
      const userId = user.id;
      
      const url = editingAsset 
        ? `${getApiUrl(API_CONFIG.ENDPOINTS.ASSETS)}/${editingAsset.id}?userId=${userId}`
        : `${getApiUrl(API_CONFIG.ENDPOINTS.ASSETS)}?userId=${userId}`;
      
      const method = editingAsset ? 'PUT' : 'POST';
      
      const response = await fetch(url, {
        method,
        headers: getAuthHeaders(),
        body: JSON.stringify(assetData)
      });

      if (!response.ok) {
        throw new Error(`Erreur ${response.status}`);
      }

      await fetchAllData();
      setShowAssetModal(false);
      setEditingAsset(null);
    } catch (err) {
      console.error('Erreur:', err);
      throw err;
    }
  };

  const handleAssetEdit = (asset: Asset) => {
    setEditingAsset(asset);
    setShowAssetModal(true);
  };

  const handleAssetDelete = async (id: number) => {
    if (!confirm('Êtes-vous sûr de vouloir supprimer cet actif ?')) {
      return;
    }

    try {
      const userStr = sessionStorage.getItem('user');
      if (!userStr) return;
      
      const user = JSON.parse(userStr);
      const userId = user.id;
      
      const response = await fetch(
        `${getApiUrl(API_CONFIG.ENDPOINTS.ASSETS)}/${id}?userId=${userId}`,
        { method: 'DELETE', headers: getAuthHeaders() }
      );

      if (!response.ok) throw new Error('Erreur lors de la suppression');

      await fetchAllData();
    } catch (err) {
      console.error('Erreur:', err);
      alert('Impossible de supprimer l\'actif');
    }
  };

  // HANDLERS PASSIFS
  const handleLiabilitySubmit = async (liabilityData: LiabilityFormData) => {
    try {
      const userStr = sessionStorage.getItem('user');
      if (!userStr) return;
      
      const user = JSON.parse(userStr);
      const userId = user.id;

      if (editingLiability) {
        await liabilityService.update(editingLiability.id, liabilityData, userId);
      } else {
        await liabilityService.create(liabilityData, userId);
      }

      await fetchAllData();
      setShowLiabilityModal(false);
      setEditingLiability(null);
    } catch (err) {
      console.error('Erreur:', err);
      throw err;
    }
  };

  const handleLiabilityEdit = (liability: Liability) => {
    setEditingLiability(liability);
    setShowLiabilityModal(true);
  };

  const handleLiabilityDelete = async (id: number) => {
    if (!confirm('Êtes-vous sûr de vouloir supprimer ce passif ?')) {
      return;
    }

    try {
      const userStr = sessionStorage.getItem('user');
      if (!userStr) return;
      
      const user = JSON.parse(userStr);
      const userId = user.id;

      await liabilityService.delete(id, userId);
      await fetchAllData();
    } catch (err) {
      console.error('Erreur:', err);
      alert('Impossible de supprimer le passif');
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('fr-CA', {
      style: 'currency',
      currency: 'CAD'
    }).format(amount);
  };

  return (
    <div className="min-h-screen bg-[#0f0f1e] text-white font-[family-name:var(--font-inter)]">
      {/* Cercles flous */}
      <div className="fixed top-0 left-0 w-full h-full overflow-hidden pointer-events-none">
        <div className="absolute top-[-20%] left-[-10%] w-[600px] h-[600px] bg-purple-600/30 rounded-full blur-[120px]"></div>
        <div className="absolute bottom-[-20%] right-[-10%] w-[700px] h-[700px] bg-pink-600/30 rounded-full blur-[120px]"></div>
      </div>

      <div className="relative z-10 max-w-7xl mx-auto px-6 py-12">
        {/* En-tête */}
        <header className={`mb-12 transition-all duration-700 ${mounted ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
          <h1 className="text-5xl font-bold mb-2 font-[family-name:var(--font-playfair)]">
            💎 Mon Patrimoine
          </h1>
          <p className="text-gray-400 text-lg">Vue complète de vos actifs et passifs</p>
        </header>

        {/* KPIs Patrimoine Net */}
        <section className="mb-8 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
          <div className="bg-gradient-to-br from-green-600/20 to-green-800/20 backdrop-blur-xl border border-green-500/30 rounded-2xl p-6">
            <div className="text-green-400 text-sm mb-2">Total Actifs</div>
            <div className="text-3xl font-bold">{formatCurrency(totalAssetValue)}</div>
            <div className="text-xs text-gray-400 mt-1">Actifs manuels</div>
          </div>

          <div className={`bg-gradient-to-br backdrop-blur-xl border rounded-2xl p-6 ${
            transactionBalance >= 0
              ? 'from-emerald-600/20 to-emerald-800/20 border-emerald-500/30'
              : 'from-orange-600/20 to-orange-800/20 border-orange-500/30'
          }`}>
            <div className={`text-sm mb-2 ${
              transactionBalance >= 0 ? 'text-emerald-400' : 'text-orange-400'
            }`}>
              {transactionBalance >= 0 ? '💰 Solde Transactions' : '⚠️ Solde Transactions'}
            </div>
            <div className="text-3xl font-bold">{formatCurrency(transactionBalance)}</div>
            <div className="text-xs text-gray-400 mt-1">
              {transactionBalance >= 0 ? 'Revenus > Dépenses' : 'Dépenses > Revenus'}
            </div>
          </div>

          <div className="bg-gradient-to-br from-red-600/20 to-red-800/20 backdrop-blur-xl border border-red-500/30 rounded-2xl p-6">
            <div className="text-red-400 text-sm mb-2">Total Dettes</div>
            <div className="text-3xl font-bold">{formatCurrency(totalDebt)}</div>
            <div className="text-xs text-gray-400 mt-1">Passifs cumulés</div>
          </div>

          <div className="bg-gradient-to-br from-blue-600/20 to-blue-800/20 backdrop-blur-xl border border-blue-500/30 rounded-2xl p-6">
            <div className="text-blue-400 text-sm mb-2">Patrimoine Net</div>
            <div className="text-3xl font-bold">{formatCurrency(netWorth)}</div>
            <div className="text-xs text-gray-400 mt-1">Actifs - Dettes</div>
          </div>

          <div className="bg-gradient-to-br from-purple-600/20 to-purple-800/20 backdrop-blur-xl border border-purple-500/30 rounded-2xl p-6">
            <div className="text-purple-400 text-sm mb-2">Utilisation Crédit</div>
            <div className="text-3xl font-bold">{creditUtilization.toFixed(1)}%</div>
            <div className="text-xs text-gray-400 mt-1">Taux d'endettement</div>
          </div>
        </section>

        {/* Onglets */}
        <div className="mb-6 flex gap-4">
          <button
            onClick={() => setActiveTab('actifs')}
            className={`px-6 py-3 rounded-xl font-semibold transition-all ${
              activeTab === 'actifs'
                ? 'bg-blue-600 text-white'
                : 'bg-white/10 text-gray-400 hover:bg-white/20'
            }`}
          >
            📈 Actifs ({assets.length})
          </button>
          <button
            onClick={() => setActiveTab('dettes')}
            className={`px-6 py-3 rounded-xl font-semibold transition-all ${
              activeTab === 'dettes'
                ? 'bg-red-600 text-white'
                : 'bg-white/10 text-gray-400 hover:bg-white/20'
            }`}
          >
            💳 Dettes ({liabilities.length})
          </button>
        </div>

        {/* Contenu selon onglet */}
        {activeTab === 'actifs' ? (
          <section>
            <AssetList
              assets={assets}
              totalValue={totalAssetValue}
              isLoading={assetsLoading}
              error={assetsError}
              onEdit={handleAssetEdit}
              onDelete={handleAssetDelete}
              onAddNew={() => {
                setEditingAsset(null);
                setShowAssetModal(true);
              }}
            />
          </section>
        ) : (
          <section>
            <div className="mb-6 flex justify-between items-center">
              <h2 className="text-2xl font-bold">Vos Dettes</h2>
              <button
                onClick={() => {
                  setEditingLiability(null);
                  setShowLiabilityModal(true);
                }}
                className="px-6 py-3 bg-red-600 hover:bg-red-700 text-white rounded-xl font-semibold transition-all"
              >
                + Ajouter une dette
              </button>
            </div>

            {liabilitiesLoading ? (
              <div className="text-center py-12 text-gray-400">
                Chargement des passifs...
              </div>
            ) : liabilities.length === 0 ? (
              <div className="text-center py-12">
                <p className="text-gray-400 text-lg mb-4">Aucun passif enregistré</p>
                <button
                  onClick={() => setShowLiabilityModal(true)}
                  className="px-6 py-3 bg-red-600 hover:bg-red-700 text-white rounded-xl font-semibold"
                >
                  Ajouter votre première dette
                </button>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {liabilities.map((liability) => (
                  <LiabilityCard
                    key={liability.id}
                    liability={liability}
                    onEdit={handleLiabilityEdit}
                    onDelete={handleLiabilityDelete}
                  />
                ))}
              </div>
            )}
          </section>
        )}

        {/* Insights IA */}
        {(assets.length > 0 || liabilities.length > 0) && (
          <section className="mt-12">
            <AIPortfolioInsights assetCount={assets.length} />
          </section>
        )}
      </div>

      {/* Modals */}
      <AssetModal
        isOpen={showAssetModal}
        onClose={() => {
          setShowAssetModal(false);
          setEditingAsset(null);
        }}
        onSubmit={handleAssetSubmit}
        editingAsset={editingAsset}
      />

      <LiabilityModal
        isOpen={showLiabilityModal}
        onClose={() => {
          setShowLiabilityModal(false);
          setEditingLiability(null);
        }}
        onSubmit={handleLiabilitySubmit}
        liability={editingLiability}
      />
    </div>
  );
}
